using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EventActivityApp.Domain;
namespace EventActivityApp.Data;


public class DbTest
{
    [Fact]
    public void CanConnectToDatabase()
    {
        using var conn = new Db().OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        Assert.Equal(1, Convert.ToInt32(cmd.ExecuteScalar()));
    }

}