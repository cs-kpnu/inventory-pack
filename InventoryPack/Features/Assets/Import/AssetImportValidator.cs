using System.Text.RegularExpressions;
using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static partial class AssetImportValidator
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var candidates = new List<ParsedAssetRow>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
        {
            var reasons = GetRejectionReasons(row);
            if (reasons.Count > 0)
                rejected.Add(CreateRejected(row, reasons));
            else
                candidates.Add(row);
        }

        var conflictingCodes = candidates
            .SelectMany(r => r.InventoryNumbers.Select(c => (Code: c, Row: r)))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => NormalizeName(x.Row.Name)).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var codeCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ledgerEntries = new List<LedgerEntry>();

        foreach (var row in candidates)
            if (row.InventoryNumbers.Any(conflictingCodes.Contains))
                rejected.Add(CreateRejected(row, [RejectionReason.DuplicateCode]));
            else
                ledgerEntries.Add(CreateLedgerEntry(row, codeCounters));

        return new ImportResult(ledgerEntries, rejected);
    }

    private static LedgerEntry CreateLedgerEntry(ParsedAssetRow row, Dictionary<string, int> counters)
    {
        var qty = row.Quantity!.Value;
        var entry = new LedgerEntry
        {
            Name = row.Name ?? string.Empty,
            Mvo = row.Mvo ?? string.Empty,
            Subaccount = row.Subaccount ?? string.Empty,
            Quantity = qty
        };

        if (qty % 1 == 0)
        {
            var intQty = (int)qty;
            var isItemized = row.InventoryNumbers.Count == intQty;
            for (var i = 0; i < intQty; i++)
            {
                var code = isItemized ? row.InventoryNumbers[i] : row.InventoryNumbers[0];
                counters.TryGetValue(code, out var current);
                counters[code] = ++current;

                entry.Assets.Add(new Asset
                {
                    InventoryNumber = code,
                    UnitIndex = current,
                    LedgerEntry = entry
                });
            }
        }
        else
        {
            foreach (var code in row.InventoryNumbers)
            {
                counters.TryGetValue(code, out var current);
                counters[code] = ++current;

                entry.Assets.Add(new Asset
                {
                    InventoryNumber = code,
                    UnitIndex = current,
                    LedgerEntry = entry
                });
            }
        }

        return entry;
    }

    private static string NormalizeName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : WhitespaceRegex().Replace(name.Trim().ToLowerInvariant().TrimEnd('.', ',', ';', ':', '-', ' '), " ");
    }

    private static RejectedRow CreateRejected(ParsedAssetRow row, IEnumerable<RejectionReason> reasons)
    {
        var rejected = new RejectedRow
        {
            RowNumber = row.RowNumber,
            RawText = row.RawText,
            Mvo = row.Mvo,
            Subaccount = row.Subaccount,
            Quantity = row.Quantity
        };

        foreach (var reason in reasons.Distinct())
            rejected.Reasons.Add(new RejectedRowReason
            {
                Reason = reason,
                RejectedRow = rejected
            });

        return rejected;
    }

    private static List<RejectionReason> GetRejectionReasons(ParsedAssetRow row)
    {
        var reasons = new List<RejectionReason>();

        if (string.IsNullOrEmpty(row.Mvo) || string.IsNullOrEmpty(row.Subaccount))
            reasons.Add(RejectionReason.MissingContext);

        if (row.Quantity is null or <= 0)
            reasons.Add(RejectionReason.InvalidQuantity);

        if (row.InventoryNumbers.Count == 0)
            reasons.Add(RejectionReason.NoSingleCode);

        if (row.Quantity is > 0 && row.Quantity % 1 == 0 && row.InventoryNumbers.Count > 1 &&
            row.InventoryNumbers.Count != (int)row.Quantity)
            reasons.Add(RejectionReason.CodeQuantityMismatch);

        return reasons;
    }
}