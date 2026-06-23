using Microsoft.Data.Sqlite;

namespace FileRenameAssistant.Data;

public sealed class AppDatabase
{
    public string DatabasePath { get; }
    public string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();

    public AppDatabase()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FileRenameAssistant");

        Directory.CreateDirectory(folder);
        DatabasePath = Path.Combine(folder, "file-rename-assistant.db");
        Initialize();
    }

    private void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
        CREATE TABLE IF NOT EXISTS RuleProfiles (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            JsonConfig TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS OperationHistory (
            Id TEXT PRIMARY KEY,
            OperationType TEXT NOT NULL,
            Summary TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            Status TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS OperationItems (
            Id TEXT PRIMARY KEY,
            HistoryId TEXT NOT NULL,
            OriginalPath TEXT NOT NULL,
            NewPath TEXT NOT NULL,
            OriginalName TEXT NOT NULL,
            NewName TEXT NOT NULL,
            Status TEXT NOT NULL,
            ErrorMessage TEXT NOT NULL,
            FOREIGN KEY(HistoryId) REFERENCES OperationHistory(Id)
        );
        """;
        command.ExecuteNonQuery();
    }
}
