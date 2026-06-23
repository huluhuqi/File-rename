using System.Text.Json;
using FileRenameAssistant.Models;
using Microsoft.Data.Sqlite;

namespace FileRenameAssistant.Data;

public sealed class RuleProfileRepository
{
    private readonly AppDatabase _database;

    public RuleProfileRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task SaveAsync(RuleProfile profile)
    {
        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
        INSERT INTO RuleProfiles (Id, Name, JsonConfig, CreatedAt, UpdatedAt)
        VALUES ($id, $name, $json, $createdAt, $updatedAt)
        ON CONFLICT(Id) DO UPDATE SET
            Name = excluded.Name,
            JsonConfig = excluded.JsonConfig,
            UpdatedAt = excluded.UpdatedAt;
        """;
        command.Parameters.AddWithValue("$id", profile.Id);
        command.Parameters.AddWithValue("$name", profile.Name);
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(profile.Config));
        command.Parameters.AddWithValue("$createdAt", profile.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", profile.UpdatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<RuleProfile>> LoadAllAsync()
    {
        var result = new List<RuleProfile>();

        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
        SELECT Id, Name, JsonConfig, CreatedAt, UpdatedAt
        FROM RuleProfiles
        ORDER BY UpdatedAt DESC;
        """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var config = JsonSerializer.Deserialize<RuleProfileConfig>(reader.GetString(2)) ?? new RuleProfileConfig();
            result.Add(new RuleProfile
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Config = config,
                CreatedAt = DateTime.Parse(reader.GetString(3)),
                UpdatedAt = DateTime.Parse(reader.GetString(4))
            });
        }

        return result;
    }

    public async Task DeleteAsync(string id)
    {
        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM RuleProfiles WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        await command.ExecuteNonQueryAsync();
    }
}
