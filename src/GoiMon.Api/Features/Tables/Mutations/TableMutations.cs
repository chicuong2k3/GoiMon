using GoiMon.Api.Features.Tables.Models;
using GoiMon.Api.Domain.Enums;
using GoiMon.Api.Infrastructure.Services;
using HotChocolate.Subscriptions;

namespace GoiMon.Api.Features.Tables.Mutations;

[ExtendObjectType("Mutation")]
public sealed class TableMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<TableSlot> CreateTableSlot(CreateTableSlotInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        var table = new TableSlot(Guid.NewGuid(), input.Code, input.Name, input.Capacity);
        db.TableSlots.Add(table);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        return table;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<TableSlot?> UpdateTableSlot(UpdateTableSlotInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        var table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (table is null)
        {
            return null;
        }

        table.Update(input.Code, input.Name, input.Capacity);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        return table;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<TableSlot?> DeactivateTableSlot(DeactivateTableSlotInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        var table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (table is null)
        {
            return null;
        }

        var activeOrderExists = await db.Orders.AnyAsync(o =>
            o.TableSlotId == table.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

        if (activeOrderExists)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetCode("TABLE_HAS_ACTIVE_ORDER")
                    .SetMessage("Cannot deactivate table slot with active order.")
                    .Build());
        }

        table.Deactivate();
        table.SetState(TableServiceState.Available);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        return table;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<TableSlot?> SetTableState(SetTableStateInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        var table = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (table is null)
        {
            return null;
        }

        if (input.State == TableServiceState.Available)
        {
            var activeOrderExists = await db.Orders.AnyAsync(o =>
                o.TableSlotId == table.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

            if (activeOrderExists)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetCode("TABLE_HAS_ACTIVE_ORDER")
                        .SetMessage("Cannot set table to Available while an active order exists.")
                        .Build());
            }
        }

        table.SetState(input.State);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, table);
        return table;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> MergeTableSlots(MergeTableSlotsInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        if (input.SourceTableSlotId == input.TargetTableSlotId)
        {
            throw new GraphQLException(ErrorBuilder.New().SetCode("INVALID_MERGE").SetMessage("Source and target table must be different.").Build());
        }

        var source = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.SourceTableSlotId);
        var target = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.TargetTableSlotId);
        if (source is null || target is null)
        {
            return null;
        }

        var sourceOrder = await db.Orders.FirstOrDefaultAsync(o =>
            o.TableSlotId == source.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

        if (sourceOrder is null)
        {
            throw new GraphQLException(ErrorBuilder.New().SetCode("SOURCE_EMPTY").SetMessage("Source table has no active order.").Build());
        }

        var targetOccupied = await db.Orders.AnyAsync(o =>
            o.TableSlotId == target.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

        if (targetOccupied)
        {
            throw new GraphQLException(ErrorBuilder.New().SetCode("TARGET_OCCUPIED").SetMessage("Target table already has an active order.").Build());
        }

        sourceOrder.AssignTableSlot(target.Id);
        source.SetState(TableServiceState.Available);
        target.SetState(TableServiceState.Occupied);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, source);
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, target);
        return sourceOrder;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<TableSlot?> SplitTableSlot(SplitTableSlotInput input, [Service(ServiceKind.Pooled)] AppDbContext db, [Service] ITopicEventSender eventSender)
    {
        var source = await db.TableSlots.FirstOrDefaultAsync(x => x.Id == input.SourceTableSlotId);
        if (source is null)
        {
            return null;
        }

        var clone = new TableSlot(Guid.NewGuid(), input.NewCode, input.NewName, input.Capacity);
        db.TableSlots.Add(clone);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(TableSubscriptionTopics.TableSlotChanged, clone);
        return clone;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> SplitBill(SplitBillInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var sourceOrder = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == input.OrderId);

        if (sourceOrder is null)
        {
            return null;
        }

        var itemIds = input.ItemIds?.Distinct().ToList() ?? new List<Guid>();
        if (itemIds.Count == 0)
        {
            throw new GraphQLException(ErrorBuilder.New().SetCode("EMPTY_SPLIT_ITEMS").SetMessage("No items selected for bill split.").Build());
        }

        var selectedItems = sourceOrder.Items.Where(i => itemIds.Contains(i.Id)).ToList();
        if (selectedItems.Count == 0)
        {
            throw new GraphQLException(ErrorBuilder.New().SetCode("ITEMS_NOT_FOUND").SetMessage("Selected items are not in source order.").Build());
        }

        var newOrder = new Order(Guid.NewGuid());
        newOrder.AssignTableSlot(sourceOrder.TableSlotId);

        foreach (var item in selectedItems)
        {
            newOrder.AddItem(item.ProductId, item.ProductName, item.Qty, item.UnitPrice, item.UnitName, item.ComboId, item.ComboName);
            sourceOrder.RemoveItem(item.Id);
        }

        db.Orders.Add(newOrder);
        await db.SaveChangesAsync();
        return newOrder;
    }
}
