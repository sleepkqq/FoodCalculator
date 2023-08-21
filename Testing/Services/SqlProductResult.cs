using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text;
using Testing.Models;
using Testing.Repositories;

namespace Testing.Services;

 public class SqlProductResult : SqlService, IProductResultRepository
{
    private readonly GlobalVariable globalVar = GlobalVariable.Instance;

    public SqlProductResult(string connectionString, List<string> tableStructure) : base(connectionString, tableStructure)
    {
    }

    public async Task ManageProductResultTable()
    {
        using (SqlConnection connection = new(connectionString))
        {
            connection.Open();

            string tableName = "ProductResult";

            if (TableExists(connection, tableName))
            {
                DropTable(connection, tableName);
            }

            CreateTable(connection, tableName);
        }
        globalVar.Value += 1;
    }


    public async Task InitProductResultTable(ConcurrentQueue<ProductResult> productResult)
    {
        using (SqlConnection connection = new(connectionString))
        {
            connection.Open();

            DataTable dataTable = CreateDataTable();

            while (!productResult.IsEmpty)
            {
                if (productResult.TryDequeue(out ProductResult diary))
                {
                    DataRow row = dataTable.NewRow();
                    row["ObjectId"] = diary.ObjectId;
                    row["ProductId"] = diary.ProductId;
                    StringBuilder result = new();
                    foreach (var product in tableStructure)
                    {
                        if (diary.ProductValues[product] != null)
                        {
                            decimal value = (decimal)diary.ProductValues[product];
                            result.Append($"<Ing_x0028_{product}_x0029_>{value.ToString(CultureInfo.InvariantCulture)}</Ing_x0028_{product}_x0029_>");
                        }
                    }
                    row["SpecialPurposeColumns"] = result.ToString();
                    dataTable.Rows.Add(row);
                }
            }

            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "ProductResult";
                bulkCopy.BulkCopyTimeout = 1000;
                globalVar.Value += 1;
                bulkCopy.WriteToServer(dataTable);
                globalVar.Value += 2;
            }
        }
    }

    private void CreateTable(SqlConnection connection, string tableName)
    {
        var createTableBuilder = new StringBuilder($"CREATE TABLE {tableName} (");
        createTableBuilder.AppendLine("ObjectId UNIQUEIDENTIFIER PRIMARY KEY,");
        createTableBuilder.AppendLine("  ProductId INTEGER,");

        foreach (var product in tableStructure)
        {
            createTableBuilder.AppendLine($"  [Ing({product})] DECIMAL(28,12) SPARSE NULL,");
        }
        createTableBuilder.AppendLine("SpecialPurposeColumns XML COLUMN_SET FOR ALL_SPARSE_COLUMNS)");

        using (var createTableCommand = new SqlCommand(createTableBuilder.ToString(), connection))
        {
            createTableCommand.ExecuteNonQuery();
        }
    }

    private DataTable CreateDataTable()
    {
        DataTable dataTable = new("ProductResult");
        dataTable.Columns.Add("ObjectId", typeof(Guid));
        dataTable.Columns.Add("ProductId", typeof(int));
        dataTable.Columns.Add("SpecialPurposeColumns");

        return dataTable;
    }
}
