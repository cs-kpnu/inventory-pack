using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InventoryPack.Data;

public class SqliteConnectionInterceptor : DbConnectionInterceptor
{
    public static readonly SqliteConnectionInterceptor Instance = new();

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SqliteCustomFunctions.Register(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        SqliteCustomFunctions.Register(connection);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }
}