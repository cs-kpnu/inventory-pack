namespace InventoryPack.Features.Assets;

public record RegisterAssetsRequest(
    Guid CodeGroupId,
    Guid RequestId,
    int Count
);