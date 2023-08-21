using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Text;
using Testing.Models;
using Testing.Repositories;

namespace Testing.Services;

public class SqlChemicalResult : SqlService, IChemicalResultRepository
{
    private readonly GlobalVariable globalVar = GlobalVariable.Instance;

    public SqlChemicalResult(string connectionString, List<string> tableStructure) : base(connectionString, tableStructure)
    {
    }

    public async Task ManageChemicalResultTable()
    {
        using (SqlConnection connection = new(connectionString))
        {
            connection.Open();

            string tableName = "ChemicalResult";
            bool tableExists = TableExists(connection, tableName);

            if (tableExists)
            {
                DropTable(connection, tableName);
            }

            CreateTable(connection, tableName);
        }
        globalVar.Value += 1;
    }

    public async Task InitChemicalResultTable(ConcurrentQueue<ChemicalResult> chemicalResult)
    {
        using (SqlConnection connection = new(connectionString))
        {
            connection.Open();

            DataTable dataTable = CreateDataTable();

            while (!chemicalResult.IsEmpty)
            {
                if (chemicalResult.TryDequeue(out ChemicalResult diary))
                {
                    DataRow row = dataTable.NewRow();
                    row["ObjectId"] = diary.ObjectId;
                    row["ProductId"] = diary.ProductId;
                    for (int i = 0; i < tableStructure.Count; i++)
                    {
                        row[tableStructure[i]] = diary.ChemicalValues[i];
                    }
                    dataTable.Rows.Add(row);
                }
            }

            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "ChemicalResult";
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

        foreach (var chemical in tableStructure)
        {
            createTableBuilder.AppendLine($"  {chemical} DECIMAL(28,12),");
        }

        createTableBuilder.Remove(createTableBuilder.Length - 3, 1); // Удаление последней запятой
        createTableBuilder.AppendLine(")");

        using (var createTableCommand = new SqlCommand(createTableBuilder.ToString(), connection))
        {
            createTableCommand.ExecuteNonQuery();
        }
    }

    private DataTable CreateDataTable()
    {
        DataTable dataTable = new("ChemicalResult");
        dataTable.Columns.Add("ObjectId", typeof(Guid));
        dataTable.Columns.Add("ProductId", typeof(int));

        foreach (var chemical in tableStructure)
        {
            dataTable.Columns.Add(chemical, typeof(decimal));
        }

        return dataTable;
    }
}
