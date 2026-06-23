using FileRenameAssistant.Models;
using Microsoft.Data.Sqlite;

namespace FileRenameAssistant.Data;

public sealed class HistoryRepository
{
    private readonly AppDatabase _database;

    public HistoryRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task SaveAsync(OperationHistory history)
    {
        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        var historyCommand = connection.CreateCommand();
        historyCommand.Transaction = transaction;
        historyCommand.CommandText = """
        INSERT INTO OperationHistory (Id, OperationType, Summary, CreatedAt, Status)
        VALUES ($id, $type, $summary, $createdAt, $status);
        """;
        historyCommand.Parameters.AddWithValue("$id", history.Id);
        historyCommand.Parameters.AddWithValue("$type", history.OperationType);
        historyCommand.Parameters.AddWithValue("$summary", history.Summary);
        historyCommand.Parameters.AddWithValue("$createdAt", history.CreatedAt.ToString("O"));
        historyCommand.Parameters.AddWithValue("$status", history.Status);
        await historyCommand.ExecuteNonQueryAsync();

        foreach (var item in history.Items)
        {
            var itemCommand = connection.CreateCommand();
            itemCommand.Transaction = transaction;
            itemCommand.CommandText = """
            INSERT INTO OperationItems
            (Id, HistoryId, OriginalPath, NewPath, OriginalName, NewName, Status, ErrorMessage)
            VALUES ($id, $historyId, $originalPath, $newPath, $originalName, $newName, $status, $errorMessage);
            """;
            itemCommand.Parameters.AddWithValue("$id", item.Id);
            itemCommand.Parameters.AddWithValue("$historyId", history.Id);
            itemCommand.Parameters.AddWithValue("$originalPath", item.OriginalPath);
            itemCommand.Parameters.AddWithValue("$newPath", item.NewPath);
            itemCommand.Parameters.AddWithValue("$originalName", item.OriginalName);
            itemCommand.Parameters.AddWithValue("$newName", item.NewName);
            itemCommand.Parameters.AddWithValue("$status", item.Status);
            itemCommand.Parameters.AddWithValue("$errorMessage", item.ErrorMessage);
            await itemCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task<OperationHistory?> LoadLastAsync()
    {
        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
        SELECT Id, OperationType, Summary, CreatedAt, Status
        FROM OperationHistory
        ORDER BY CreatedAt DESC
        LIMIT 1;
        """;

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var history = new OperationHistory
        {
            Id = reader.GetString(0),
            OperationType = reader.GetString(1),
            Summary = reader.GetString(2),
            CreatedAt = DateTime.Parse(reader.GetString(3)),
            Status = reader.GetString(4)
        };

        await reader.DisposeAsync();

        var itemCommand = connection.CreateCommand();
        itemCommand.CommandText = """
        SELECT OriginalPath, NewPath
        FROM OperationItems
        WHERE HistoryId = $historyId
        ORDER BY rowid ASC;
        """;
        itemCommand.Parameters.AddWithValue("$historyId", history.Id);

        await using var itemReader = await itemCommand.ExecuteReaderAsync();
        while (await itemReader.ReadAsync())
        {
            history.Items.Add(new OperationItem
            {
                OriginalPath = itemReader.GetString(0),
                NewPath = itemReader.GetString(1)
            });
        }

        return history;
    }

    public async Task<IReadOnlyList<string>> LoadRecentSummariesAsync(int limit = 5)
    {
        var result = new List<string>();

        await using var connection = new SqliteConnection(_database.ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
        SELECT Summary, CreatedAt
        FROM OperationHistory
        ORDER BY CreatedAt DESC
        LIMIT $limit;
        """;
        command.Parameters.AddWithValue("$limit", limit);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var summary = reader.GetString(0);
            var createdAt = DateTime.Parse(reader.GetString(1));
            result.Add($"{createdAt:yyyy-MM-dd HH:mm}  {summary}");
        }

        return result;
    }
}
