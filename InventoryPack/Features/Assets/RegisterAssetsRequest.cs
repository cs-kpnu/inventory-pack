namespace InventoryPack.Features.Assets;

public record RegisterAssetsRequest(
    Guid LedgerEntryId,
    Guid CodeGroupId,
    Guid RequestId,
    int Count
);
