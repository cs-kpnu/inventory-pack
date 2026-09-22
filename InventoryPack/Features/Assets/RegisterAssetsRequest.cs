using System.Text.Json.Serialization;

namespace InventoryPack.Features.Assets;

public record RegisterAssetsRequest(
    Guid CodeGroupId,
    Guid RequestId,
    int Count,
    [property: JsonRequired] string? RegisteredForMvo
);