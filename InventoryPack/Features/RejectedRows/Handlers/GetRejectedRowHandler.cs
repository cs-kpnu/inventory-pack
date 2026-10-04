using InventoryPack.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.RejectedRows.Handlers;

public class GetRejectedRowHandler(AppDbContext db)
{
    public async Task<RejectedRowDto?> HandleAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.RejectedRows.AsNoTracking()
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync(ct);

        if (row is null)
            return null;

        var rows = await RejectedRowMapping.MapAsync(db, [row], ct);
        return rows[0];
    }
}
