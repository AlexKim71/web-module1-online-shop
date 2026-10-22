using Domain.Interfaces;
using Domain.Models;
using Domain.Validation;
using Microsoft.Data.Sqlite;

namespace Infrastructure;

public class SqliteRequestRepository : IRequestRepository
{
    private readonly string _cs;
    public SqliteRequestRepository(string dbPath) => _cs = $"Data Source={dbPath};Foreign Keys=True";
    private SqliteConnection Open() { var c = new SqliteConnection(_cs); c.Open(); return c; }

    public async Task<IReadOnlyList<Request>> GetAllAsync(CancellationToken ct = default)
    {
        var list = new List<Request>();
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT q.Id, q.ResourceId, q.CustomerName, q.Quantity, q.UnitPriceMinor, q.CreatedAtUtc, q.Status, r.Name FROM Requests q JOIN Resources r ON r.Id = q.ResourceId ORDER BY q.Id DESC";
        using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new Request { Id = rd.GetInt32(0), ResourceId = rd.GetInt32(1), CustomerName = rd.GetString(2), Quantity = rd.GetInt32(3), UnitPriceMinor = rd.GetInt64(4), CreatedAtUtc = DateTime.Parse(rd.GetString(5)), Status = Enum.Parse<RequestStatus>(rd.GetString(6)), ResourceName = rd.GetString(7) });
        }
        return list;
    }

    public async Task<int> CreateAsync(int resourceId, string customerName, int quantity, CancellationToken ct = default)
    {
        DomainRules.ValidateRequestInput(customerName, quantity);
        using var c = Open();
        using var tx = c.BeginTransaction();
        try
        {
            long price; int available;
            using (var read = c.CreateCommand())
            {
                read.Transaction = tx; read.CommandText = "SELECT PriceMinor, Available FROM Resources WHERE Id=$id"; read.Parameters.AddWithValue("$id", resourceId);
                using var rd = await read.ExecuteReaderAsync(ct);
                if (!await rd.ReadAsync(ct)) throw new InvalidOperationException("Ресурс не знайдено.");
                price = rd.GetInt64(0); available = rd.GetInt32(1);
            }
            if (quantity > available) throw new InvalidOperationException("Недостатньо одиниць.");
            using (var update = c.CreateCommand())
            {
                update.Transaction = tx; update.CommandText = "UPDATE Resources SET Available = Available - $qty WHERE Id=$id AND Available >= $qty";
                update.Parameters.AddWithValue("$qty", quantity); update.Parameters.AddWithValue("$id", resourceId);
                if (await update.ExecuteNonQueryAsync(ct) != 1) throw new InvalidOperationException("Помилка оновлення.");
            }
            using (var insert = c.CreateCommand())
            {
                insert.Transaction = tx; insert.CommandText = "INSERT INTO Requests (ResourceId, CustomerName, Quantity, UnitPriceMinor, CreatedAtUtc, Status) VALUES ($resId, $name, $qty, $price, $date, 'Active'); SELECT last_insert_rowid();";
                insert.Parameters.AddWithValue("$resId", resourceId); insert.Parameters.AddWithValue("$name", customerName); insert.Parameters.AddWithValue("$qty", quantity); insert.Parameters.AddWithValue("$price", price); insert.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("O"));
                var id = Convert.ToInt32(await insert.ExecuteScalarAsync(ct));
                tx.Commit(); return id;
            }
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task<bool> CancelAsync(int requestId, CancellationToken ct = default)
    {
        using var c = Open();
        using var tx = c.BeginTransaction();
        try
        {
            int resourceId; int quantity;
            using (var read = c.CreateCommand())
            {
                read.Transaction = tx; read.CommandText = "SELECT ResourceId, Quantity FROM Requests WHERE Id=$id AND Status='Active'"; read.Parameters.AddWithValue("$id", requestId);
                using var rd = await read.ExecuteReaderAsync(ct);
                if (!await rd.ReadAsync(ct)) return false;
                resourceId = rd.GetInt32(0); quantity = rd.GetInt32(1);
            }
            using (var update = c.CreateCommand())
            {
                update.Transaction = tx; update.CommandText = "UPDATE Requests SET Status='Cancelled' WHERE Id=$id AND Status='Active'"; update.Parameters.AddWithValue("$id", requestId);
                if (await update.ExecuteNonQueryAsync(ct) != 1) return false;
            }
            using (var restore = c.CreateCommand())
            {
                restore.Transaction = tx; restore.CommandText = "UPDATE Resources SET Available = Available + $qty WHERE Id=$id";
                restore.Parameters.AddWithValue("$qty", quantity); restore.Parameters.AddWithValue("$id", resourceId);
                await restore.ExecuteNonQueryAsync(ct);
            }
            tx.Commit(); return true;
        }
        catch { tx.Rollback(); throw; }
    }
}