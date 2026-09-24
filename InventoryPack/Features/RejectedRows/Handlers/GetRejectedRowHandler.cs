using InventoryPack.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.RejectedRows.Handlers;

public class GetRejectedRowHandler(AppDbContext db)
{
    public async Task<RejectedRowDto?> HandleAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.RejectedRows.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.RowNumber,
                r.RawText,
                r.Quantity,
                r.Unit,
                r.Mvo,
                r.Subaccount
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
            return null;

        var reasons = await db.RejectedRowReasons.AsNoTracking()
            .Where(re => re.RejectedRowId == id)
            .Select(re => re.Reason.ToString())
            .OrderBy(s => s)
            .ToListAsync(ct);

        return new RejectedRowDto(
            row.Id,
            row.RowNumber,
            row.RawText,
            row.Quantity,
            row.Unit,
            row.Mvo,
            row.Subaccount,
            reasons);
    }
}
