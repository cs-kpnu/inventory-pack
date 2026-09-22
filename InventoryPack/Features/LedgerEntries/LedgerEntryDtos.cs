using InventoryPack.Features.Assets;

namespace InventoryPack.Features.LedgerEntries;

public record LedgerEntryCodeDto(
    Guid Id,
    string Code,
    int RegisteredAssetCount,
    int SourceRowCount
);

public record LedgerEntrySummaryDto(
    Guid Id,
    int SourceRowNumber,
    string Name,
    decimal Quantity,
    string? Unit,
    string? Mvo,
    string Subaccount,
    IReadOnlyList<LedgerEntryCodeDto> Codes
);

public record LedgerEntryListResponse(
    IReadOnlyList<LedgerEntrySummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasRejectedRows
);

public record LedgerEntryDetailCodeDto(
    Guid Id,
    string Code,
    int SourceRowCount,
    IReadOnlyList<RegisteredAssetDto> Assets
);

public record LedgerEntryDetailDto(
    Guid Id,
    int SourceRowNumber,
    string SourceTitle,
    string Name,
    decimal Quantity,
    string? Unit,
    string? Mvo,
    string Subaccount,
    IReadOnlyList<LedgerEntryDetailCodeDto> Codes
);