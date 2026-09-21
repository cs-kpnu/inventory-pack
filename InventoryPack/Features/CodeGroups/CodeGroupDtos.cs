namespace InventoryPack.Features.CodeGroups;

public record CodeGroupSummaryDto(
    Guid Id,
    string Code,
    int RegisteredAssetCount,
    int SourceRowCount,
    IReadOnlyList<CodeGroupSourceRowDto> SampleRows
);

public record CodeGroupListResponse(
    IReadOnlyList<CodeGroupSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasRejectedRows
);

public record CodeGroupSourceRowDto(
    Guid LedgerEntryId,
    int SourceRowNumber,
    string Name,
    decimal Quantity,
    string? Unit,
    string? Mvo,
    string Subaccount
);

public record CodeGroupDetailDto(
    Guid Id,
    string Code,
    int RegisteredAssetCount,
    IReadOnlyList<CodeGroupSourceRowDto> SourceRows
);