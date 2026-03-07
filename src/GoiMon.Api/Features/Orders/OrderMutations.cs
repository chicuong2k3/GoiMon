using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoiMon.Api.Features.Orders.Services;
using GoiMon.Api.Domain.Enums;
using GoiMon.Api.Features.Tables;
using GoiMon.Api.Infrastructure.Services;

namespace GoiMon.Api.Features.Orders;

/// <summary>
/// GraphQL mutation extensions for order-related operations.
/// These resolvers are registered as extensions to the root Mutation type.
/// </summary>
[ExtendObjectType("Mutation")]
public class OrderMutations
{
    private readonly IOrderTelemetry _telemetry;

    public OrderMutations(IOrderTelemetry telemetry)
    {
        _telemetry = telemetry;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<CreateOrderPayload> CreateOrder(
        CreateOrderInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var errors = new List<CreateOrderValidationError>();
        var order = new Order(Guid.NewGuid());
        var selectedModifierCount = 0;
        var comboLines = input.ComboLines ?? new List<CreateOrderComboLineInput>();
        TableSlot? assignedTable = null;

        if (input.TableSlotId.HasValue)
        {
            assignedTable = await db.TableSlots
                .FirstOrDefaultAsync(x => x.Id == input.TableSlotId.Value && x.IsActive);

            if (assignedTable is null)
            {
                errors.Add(new CreateOrderValidationError("TABLE_NOT_FOUND", "Table slot does not exist or is inactive."));
            }
            else
            {
                var alreadyOccupied = await db.Orders.AnyAsync(o =>
                    o.TableSlotId == assignedTable.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

                if (alreadyOccupied)
                {
                    errors.Add(new CreateOrderValidationError("TABLE_OCCUPIED", "Table slot already has an active order."));
                }
                else
                {
                    order.AssignTableSlot(assignedTable.Id);
                    assignedTable.SetState(TableServiceState.Occupied);
                }
            }
        }

        for (var index = 0; index < input.Lines.Count; index++)
        {
            var line = input.Lines[index];
            var lineErrorCountBefore = errors.Count;

            if (line.Quantity <= 0)
            {
                errors.Add(new CreateOrderValidationError("INVALID_QTY", "Quantity must be greater than zero", index));
                continue;
            }

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId);
            if (product is null)
            {
                errors.Add(new CreateOrderValidationError("PRODUCT_NOT_FOUND", "Product does not exist", index));
                continue;
            }

            var activeVariants = await db.ProductVariants
                .Where(v => v.ProductId == product.Id && v.IsActive)
                .OrderBy(v => v.SortOrder)
                .ToListAsync();

            ProductVariant? selectedVariant = null;
            if (activeVariants.Count > 0)
            {
                if (line.VariantId is null)
                {
                    errors.Add(new CreateOrderValidationError("REQUIRED_VARIANT", "Variant selection is required for this product", index));
                    continue;
                }

                selectedVariant = activeVariants.FirstOrDefault(v => v.Id == line.VariantId.Value);
                if (selectedVariant is null)
                {
                    errors.Add(new CreateOrderValidationError("INVALID_VARIANT", "Selected variant is invalid or inactive", index));
                    continue;
                }
            }
            else if (line.VariantId is not null)
            {
                errors.Add(new CreateOrderValidationError("INVALID_VARIANT", "Product has no variants but variant was provided", index));
                continue;
            }

            var modifiers = line.Modifiers ?? new List<CreateOrderLineModifierInput>();
            if (modifiers.GroupBy(m => m.OptionId).Any(g => g.Count() > 1))
            {
                errors.Add(new CreateOrderValidationError("DUPLICATE_OPTION", "Each modifier option can only appear once per line", index));
                continue;
            }

            var optionIds = modifiers.Select(m => m.OptionId).Distinct().ToList();

            var optionRows = new List<(ModifierOption Option, ModifierGroup Group)>();
            if (optionIds.Count > 0)
            {
                var joinedRows = await (
                    from option in db.ModifierOptions
                    join modifierGroup in db.ModifierGroups on option.ModifierGroupId equals modifierGroup.Id
                    where optionIds.Contains(option.Id)
                    select new { Option = option, Group = modifierGroup })
                    .ToListAsync();

                optionRows = joinedRows.Select(x => (x.Option, x.Group)).ToList();
            }

            var optionById = optionRows.ToDictionary(x => x.Option.Id, x => x);

            foreach (var modifier in modifiers)
            {
                if (!optionById.TryGetValue(modifier.OptionId, out var pair))
                {
                    errors.Add(new CreateOrderValidationError("INVALID_OPTION", "Selected modifier option does not exist", index));
                    continue;
                }

                if (!pair.Group.IsActive || !pair.Option.IsActive)
                {
                    errors.Add(new CreateOrderValidationError("OPTION_INACTIVE", "Selected modifier option is inactive", index));
                }

                if (pair.Group.ProductId != product.Id)
                {
                    errors.Add(new CreateOrderValidationError("OPTION_NOT_ALLOWED", "Selected modifier option is not allowed for this product", index));
                }

                if (modifier.Quantity > pair.Option.MaxQty)
                {
                    errors.Add(new CreateOrderValidationError("OPTION_QTY_EXCEEDED", "Selected modifier quantity exceeds allowed maximum", index));
                }

                if (pair.Group.SelectionMode == ModifierSelectionMode.Single && modifier.Quantity > 1)
                {
                    errors.Add(new CreateOrderValidationError("GROUP_SINGLE_EXCEEDED", "Single-select group cannot have quantity greater than one", index));
                }
            }

            var activeGroups = await db.ModifierGroups
                .Where(g => g.ProductId == product.Id && g.IsActive)
                .ToListAsync();

            var selectedQtyByGroupId = new Dictionary<Guid, int>();
            foreach (var modifier in modifiers)
            {
                if (!optionById.TryGetValue(modifier.OptionId, out var pair))
                {
                    continue;
                }

                selectedQtyByGroupId[pair.Group.Id] = selectedQtyByGroupId.TryGetValue(pair.Group.Id, out var selectedQty)
                    ? selectedQty + modifier.Quantity
                    : modifier.Quantity;
            }

            foreach (var modifierGroup in activeGroups)
            {
                var selectedQty = selectedQtyByGroupId.GetValueOrDefault(modifierGroup.Id, 0);

                if (selectedQty < modifierGroup.MinSelect)
                {
                    errors.Add(new CreateOrderValidationError("GROUP_MIN_NOT_MET", $"Group '{modifierGroup.Name}' requires at least {modifierGroup.MinSelect} selection(s)", index));
                }

                if (modifierGroup.IsRequired && selectedQty == 0)
                {
                    errors.Add(new CreateOrderValidationError("GROUP_REQUIRED", $"Group '{modifierGroup.Name}' is required", index));
                }

                if (selectedQty > modifierGroup.MaxSelect)
                {
                    errors.Add(new CreateOrderValidationError("GROUP_MAX_EXCEEDED", $"Group '{modifierGroup.Name}' allows at most {modifierGroup.MaxSelect} selection(s)", index));
                }

                if (modifierGroup.SelectionMode == ModifierSelectionMode.Single && selectedQty > 1)
                {
                    errors.Add(new CreateOrderValidationError("GROUP_SINGLE_EXCEEDED", $"Group '{modifierGroup.Name}' allows only one selection", index));
                }
            }

            if (errors.Count > lineErrorCountBefore)
            {
                continue;
            }

            var basePrice = selectedVariant?.Price ?? product.Price;
            var modifiersDelta = modifiers.Sum(modifier => optionById[modifier.OptionId].Option.PriceDelta * modifier.Quantity);
            var unitPrice = basePrice + modifiersDelta;

            var productSnapshotName = selectedVariant is null
                ? product.Name
                : $"{product.Name} ({selectedVariant.Name})";

            order.AddItem(product.Id, productSnapshotName, line.Quantity, unitPrice, product.UnitName);

            var orderItem = order.Items.Last();
            foreach (var modifier in modifiers)
            {
                var selected = optionById[modifier.OptionId];
                selectedModifierCount += modifier.Quantity;
                db.OrderItemModifiers.Add(new OrderItemModifier(
                    Guid.NewGuid(),
                    orderItem.Id,
                    selected.Option.Id,
                    selected.Group.Name,
                    selected.Option.Name,
                    modifier.Quantity,
                    selected.Option.PriceDelta));
            }
        }

        for (var comboIndex = 0; comboIndex < comboLines.Count; comboIndex++)
        {
            var comboLine = comboLines[comboIndex];

            if (comboLine.Quantity <= 0)
            {
                errors.Add(new CreateOrderValidationError("INVALID_QTY", "Quantity must be greater than zero", input.Lines.Count + comboIndex));
                continue;
            }

            var combo = await db.ProductCombos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == comboLine.ComboId);

            if (combo is null)
            {
                errors.Add(new CreateOrderValidationError("COMBO_NOT_FOUND", "Combo does not exist", input.Lines.Count + comboIndex));
                continue;
            }

            if (combo.Items.Count == 0)
            {
                errors.Add(new CreateOrderValidationError("COMBO_EMPTY", "Combo has no items", input.Lines.Count + comboIndex));
                continue;
            }

            order.AddItem(
                productId: null,
                productName: combo.Name ?? "Combo",
                qty: comboLine.Quantity,
                unitPrice: combo.Price,
                unitName: null,
                comboId: combo.Id,
                comboName: combo.Name);
        }

        if (errors.Count > 0)
        {
            _telemetry.TrackValidationFailed(errors.Count, input.Lines.Count + comboLines.Count);
            return new CreateOrderPayload(null, errors);
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        _telemetry.TrackOrderCreated(input.Lines.Count + comboLines.Count, selectedModifierCount, order.Total);
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        if (assignedTable is not null)
        {
            await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, assignedTable);
        }
        return new CreateOrderPayload(order, new List<CreateOrderValidationError>());
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> CompleteOrder(
        Guid orderId,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;
        order.MarkCompleted();

        TableSlot? table = null;
        if (order.TableSlotId.HasValue)
        {
            table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == order.TableSlotId.Value);
            table?.SetState(TableServiceState.AwaitingPayment);
        }

        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        if (table is not null)
        {
            await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        }
        return order;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> MarkOrderPaid(
        Guid orderId,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;
        order.MarkPaid();

        TableSlot? table = null;
        if (order.TableSlotId.HasValue)
        {
            table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == order.TableSlotId.Value);
            table?.SetState(TableServiceState.Available);
        }

        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        if (table is not null)
        {
            await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        }
        return order;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> CancelOrder(
        Guid orderId,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;
        order.Cancel();

        TableSlot? table = null;
        if (order.TableSlotId.HasValue)
        {
            table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == order.TableSlotId.Value);
            table?.SetState(TableServiceState.Available);
        }

        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        if (table is not null)
        {
            await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        }
        return order;
    }
}

public record CreateOrderInput(List<CreateOrderLineInput> Lines, List<CreateOrderComboLineInput>? ComboLines = null, Guid? TableSlotId = null);

public record CreateOrderLineInput(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    List<CreateOrderLineModifierInput>? Modifiers = null);

public record CreateOrderComboLineInput(Guid ComboId, int Quantity);

public record CreateOrderLineModifierInput(Guid OptionId, int Quantity);

public record CreateOrderValidationError(string Code, string Message, int? LineIndex = null);

public record CreateOrderPayload(Order? Order, List<CreateOrderValidationError> Errors);
