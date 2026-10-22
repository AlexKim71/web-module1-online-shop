using Domain.Interfaces;
using Domain.Models;
using Microsoft.Data.Sqlite;

namespace Infrastructure;

public class SqliteCategoryRepository : ICategoryRepository
{
    private readonly string _cs;
    public SqliteCategoryRepository(string dbPath) => _cs = $"Data Source={dbPath};Foreign Keys=True";

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        var list = new List<Category>();
        using var c = new SqliteConnection(_cs);
        await c.OpenAsync(ct);
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM Categories ORDER BY Name";
        using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            list.Add(new Category { Id = rd.GetInt32(0), Name = rd.GetString(1) });
        return list;
    }
}