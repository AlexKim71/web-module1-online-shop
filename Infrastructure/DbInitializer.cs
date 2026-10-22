using Microsoft.Data.Sqlite;

namespace Infrastructure;

public static class DbInitializer
{
    public static void EnsureCreated(string dbPath, string initSqlPath, string seedSqlPath)
    {
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Categories'";
        var exists = checkCmd.ExecuteScalar() != null;

        if (!exists)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = File.ReadAllText(initSqlPath);
            cmd.ExecuteNonQuery();

            cmd.CommandText = File.ReadAllText(seedSqlPath);
            cmd.ExecuteNonQuery();
        }
    }
}