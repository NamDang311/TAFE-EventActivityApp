using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EventActivityApp.Domain;
namespace EventActivityApp.Data;


public class DbTest
{
    [Fact]
    public async Task CanConnectToDatabase()
    {
        await using var conn = await Db.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        Assert.Equal(1, Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }

}