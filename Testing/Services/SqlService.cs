using Microsoft.Data.SqlClient;

namespace Testing.Services;

public class SqlService
{
    public readonly string connectionString;
    public readonly List<string> tableStructure;


    public SqlService(string connectionString, List<string> tableStructure)
    {
        this.connectionString = connectionString;
        this.tableStructure = tableStructure;
    }

    public bool TableExists(SqlConnection connection, string tableName)
    {
        string query = "SELECT 1 FROM sysobjects WHERE name = @tableName";
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@tableName", tableName);
            object result = command.ExecuteScalar();
            return result != null && result != DBNull.Value;
        }
    }

    public void DropTable(SqlConnection connection, string tableName)
    {
        string query = "DROP TABLE " + tableName;
        using (var command = new SqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
    }
}
