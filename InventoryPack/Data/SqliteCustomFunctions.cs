using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace InventoryPack.Data;

public static class SqliteCustomFunctions
{
    public static bool ContainsIgnoreCase(string? source, string? pattern)
    {
        throw new InvalidOperationException("This method is only intended for use with EF Core database queries.");
    }

    public static bool EqualsIgnoreCase(string? a, string? b)
    {
        throw new InvalidOperationException("This method is only intended for use with EF Core database queries.");
    }

    public static void Register(DbConnection connection)
    {
        if (connection is SqliteConnection sqlite)
        {
            sqlite.CreateFunction("contains_ignore_case", (string? source, string? pattern) =>
            {
                if (source is null || pattern is null) return false;
                return source.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            });

            sqlite.CreateFunction("equals_ignore_case", (string? a, string? b) =>
            {
                if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
                return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}