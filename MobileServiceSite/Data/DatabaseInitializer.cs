using Microsoft.EntityFrameworkCore;

namespace MobileServiceSite.Data;

/// <summary>
/// Создаёт базу и недостающие таблицы.
/// EnsureCreated() ничего не делает, если база уже существует,
/// поэтому таблицу orders создаём отдельно через IF NOT EXISTS.
/// </summary>
public static class DatabaseInitializer
{
    private const string CreateOrdersTableSql = @"
CREATE TABLE IF NOT EXISTS orders (
    id integer PRIMARY KEY,
    client_id integer NOT NULL REFERENCES clients(id),
    device_id integer NOT NULL REFERENCES devices(id),
    description varchar(500) NOT NULL,
    price integer,
    status varchar(30) NOT NULL DEFAULT 'Created',
    created_at timestamp NOT NULL DEFAULT now(),
    price_set_at timestamp,
    paid_at timestamp
);
CREATE INDEX IF NOT EXISTS idx_orders_client_id ON orders(client_id);";

    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw(CreateOrdersTableSql);
    }
}
