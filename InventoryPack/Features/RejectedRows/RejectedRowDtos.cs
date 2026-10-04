namespace InventoryPack.Features.RejectedRows;

public record RejectedRowDto(
    Guid Id,
    int RowNumber,
    string RawText,
    decimal? Quantity,
    string? Unit,
    string? Mvo,
    string? Subaccount,
    string? ParsedName,
    int ParsedCodeCount,
    IReadOnlyList<RejectionIssueDto> Issues
);

public record RejectionIssueDto(
    Guid Id,
    string Reason,
    string? Code,
    IReadOnlyList<int> RelatedRowNumbers
);

public record RejectedRowListResponse(
    IReadOnlyList<RejectedRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyDictionary<string, int> TotalReasonCounts
);
