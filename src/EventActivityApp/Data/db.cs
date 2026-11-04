using MySqlConnector;

namespace EventActivityApp.Data;

public record Order(int OrderId, DateTime OrderDate);

public class Db
{
    private readonly string ConnectionString = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        UserID = "eventapp",
        Password = "eventpass",
        Database = "ActivitiesDb",
    }.ConnectionString;

    // open a connection; the caller is responsible for disposing it
    public MySqlConnection OpenConnection()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}
