using Microsoft.Data.SqlClient;
using System.Data;

namespace HelpDesk.Data;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
    }

    public IDbConnection Create()
    {
        return new SqlConnection(_connectionString);
    }
}