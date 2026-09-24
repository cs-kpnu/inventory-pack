namespace InventoryPack.Features.RejectedRows;

public record RejectedRowDto(
    Guid Id,
    int RowNumber,
    string RawText,
    decimal? Quantity,
    string? Unit,
    string? Mvo,
    string? Subaccount,
    IReadOnlyList<string> Reasons
);

public record RejectedRowListResponse(
    IReadOnlyList<RejectedRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyDictionary<string, int> TotalReasonCounts
);
