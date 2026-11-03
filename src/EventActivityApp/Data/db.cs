using MySqlConnector;

namespace EventActivityApp.Data;

public static class Db
{
    private static readonly string ConnectionString = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        UserID = "eventapp",
        Password = "eventpass",
        Database = "eventactivity",
    }.ConnectionString;

    // open a connection asynchronously
    public static async Task<MySqlConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
