using Domain.Interfaces;
using Domain.Models;
using Domain.Validation;
using Microsoft.Data.Sqlite;

namespace Infrastructure;

public class SqliteResourceRepository : IResourceRepository
{
    private readonly string _cs;
    public SqliteResourceRepository(string dbPath) => _cs = $"Data Source={dbPath};Foreign Keys=True";

    private SqliteConnection Open() { var c = new SqliteConnection(_cs); c.Open(); return c; }

    public async Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken ct = default)
    {
        var list = new List<Resource>();
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT r.Id, r.CategoryId, r.Code, r.Name, r.PriceMinor, r.Capacity, r.Available, r.Sku, r.Brand, c.Name FROM Resources r JOIN Categories c ON c.Id = r.CategoryId ORDER BY r.Id";
        using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new Resource { Id = rd.GetInt32(0), CategoryId = rd.GetInt32(1), Code = rd.GetString(2), Name = rd.GetString(3), PriceMinor = rd.GetInt64(4), Capacity = rd.GetInt32(5), Available = rd.GetInt32(6), Sku = rd.GetString(7), Brand = rd.GetString(8), CategoryName = rd.GetString(9) });
        }
        return list;
    }

    public async Task<Resource?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT r.Id, r.CategoryId, r.Code, r.Name, r.PriceMinor, r.Capacity, r.Available, r.Sku, r.Brand, c.Name FROM Resources r JOIN Categories c ON c.Id = r.CategoryId WHERE r.Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;
        return new Resource { Id = rd.GetInt32(0), CategoryId = rd.GetInt32(1), Code = rd.GetString(2), Name = rd.GetString(3), PriceMinor = rd.GetInt64(4), Capacity = rd.GetInt32(5), Available = rd.GetInt32(6), Sku = rd.GetString(7), Brand = rd.GetString(8), CategoryName = rd.GetString(9) };
    }

    public async Task<Resource> AddAsync(Resource r, CancellationToken ct = default)
    {
        DomainRules.ValidateResource(r);
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Resources (CategoryId, Code, Name, PriceMinor, Capacity, Available, Sku, Brand) VALUES ($categoryId, $code, $name, $price, $capacity, $available, $sku, $brand); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$categoryId", r.CategoryId); cmd.Parameters.AddWithValue("$code", r.Code); cmd.Parameters.AddWithValue("$name", r.Name);
        cmd.Parameters.AddWithValue("$price", r.PriceMinor); cmd.Parameters.AddWithValue("$capacity", r.Capacity); cmd.Parameters.AddWithValue("$available", r.Capacity);
        cmd.Parameters.AddWithValue("$sku", r.Sku); cmd.Parameters.AddWithValue("$brand", r.Brand);
        r.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        return r;
    }

    public async Task<bool> UpdateAsync(Resource r, CancellationToken ct = default)
    {
        DomainRules.ValidateResource(r);
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Resources SET CategoryId=$categoryId, Code=$code, Name=$name, PriceMinor=$price, Sku=$sku, Brand=$brand WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", r.Id); cmd.Parameters.AddWithValue("$categoryId", r.CategoryId); cmd.Parameters.AddWithValue("$code", r.Code);
        cmd.Parameters.AddWithValue("$name", r.Name); cmd.Parameters.AddWithValue("$price", r.PriceMinor); cmd.Parameters.AddWithValue("$sku", r.Sku); cmd.Parameters.AddWithValue("$brand", r.Brand);
        return await cmd.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var c = Open();
        using var check = c.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Requests WHERE ResourceId=$id";
        check.Parameters.AddWithValue("$id", id);
        if (Convert.ToInt64(await check.ExecuteScalarAsync(ct)) > 0) return false;
        using var cmd = c.CreateCommand();
        cmd.CommandText = "DELETE FROM Resources WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        return await cmd.ExecuteNonQueryAsync(ct) == 1;
    }
}