using MySqlConnector;

namespace EventActivityApp.Data;

public record Order(int OrderId, DateTime OrderDate);

public static class Db
{
    private static readonly string ConnectionString = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        UserID = "eventapp",
        Password = "eventpass",
        Database = "eventactivity",
    }.ConnectionString;

    // open a connection asynchronously; the caller is responsible for disposing it
    public static async Task<MySqlConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task<List<Order>> GetOrdersByIdAsync(int orderId)
    {
        await using var connection = await OpenConnectionAsync();

        // create a DB command and set the SQL statement with parameters
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM orders WHERE order_id = @OrderId;";
        command.Parameters.AddWithValue("@OrderId", orderId);

        // execute the command and read the results
        var orders = new List<Order>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32("order_id");
            var date = reader.GetDateTime("order_date");
            orders.Add(new Order(id, date));
        }

        return orders;
    }
}
