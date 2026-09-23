namespace InventoryPack.Data;

public static class MvoNormalizer
{
    public const int MaxLength = 150;

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    public static bool Equals(string? a, string? b)
    {
        var normA = Normalize(a);
        var normB = Normalize(b);

        if (normA is null && normB is null)
            return true;

        if (normA is null || normB is null)
            return false;

        return string.Equals(normA, normB, StringComparison.OrdinalIgnoreCase);
    }
}
