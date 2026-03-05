namespace GoiMon.Api.Features.Tables.Models;

public record CreateTableSlotInput(string Code, string Name, int Capacity);

public record UpdateTableSlotInput(Guid Id, string Code, string Name, int Capacity);

public record SetTableStateInput(Guid Id, Domain.Enums.TableServiceState State);

public record DeactivateTableSlotInput(Guid Id);

public record MergeTableSlotsInput(Guid SourceTableSlotId, Guid TargetTableSlotId);

public record SplitTableSlotInput(Guid SourceTableSlotId, string NewCode, string NewName, int Capacity);

public record SplitBillInput(Guid OrderId, IReadOnlyList<Guid> ItemIds);
