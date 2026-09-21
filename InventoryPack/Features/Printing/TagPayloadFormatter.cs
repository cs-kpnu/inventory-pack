namespace InventoryPack.Features.Printing;

public static class TagPayloadFormatter
{
    public static string Format(Guid assetId)
    {
        return $"UA1:{assetId:N}".ToUpperInvariant();
    }
}