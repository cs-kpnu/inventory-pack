namespace InventoryPack.Features.Assets;

public record RegisteredAssetDto(
    Guid Id,
    string Payload,
    DateTime AllocatedAt,
    string? RegisteredForMvo
);