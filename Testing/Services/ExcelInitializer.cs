using OfficeOpenXml;
using System.Collections;

namespace Testing.Services;

public class ExcelInitializer
{
    public readonly Hashtable products = new();
    public readonly Hashtable chemical = new();
    public readonly List<string> productTableStructure = new();
    public readonly List<string> chemicalTableStructure = new();
    private GlobalVariable globalVar = GlobalVariable.Instance;

    public async Task InitializeTables()
    {
        Task initializeProductsTableTask = Task.Run(InitializeProductsTable);
        Task initializeChemistryTableTask = Task.Run(InitializeChemistryTable);

        await Task.WhenAll(initializeChemistryTableTask, initializeProductsTableTask);
    }

    private void InitializeProductsTable()
    {
        ExcelWorksheet productsTable = new ExcelPackage(new FileInfo(@"Product.xlsx"))
            .Workbook.Worksheets["Лист1"];

        int startRow = 6;
        int endRow = productsTable.Dimension.Rows;
        int startCol = 6;
        int endCol = productsTable.Dimension.Columns;

        for (int col = 6; col < endCol; col++)
        {
            productTableStructure.Add(productsTable.Cells[2, col].Text.Trim());
        }

        for (int row = startRow; row < endRow; row++)
        {
            Hashtable indexAndValue = new();

            string rowText = productsTable.Cells[row, 2].Text.Trim();
            if (int.TryParse(rowText, out int rowIndex))
            {
                for (int col = startCol; col < endCol; col++)
                {
                    string colText = productsTable.Cells[2, col].Text.Trim();

                    string cellText = productsTable.Cells[row, col].Text.Trim();
                    if (!string.IsNullOrEmpty(cellText) && decimal.TryParse(cellText, out decimal value))
                    {
                        List<decimal> values = new()
                            {
                                value, // n (j, x)
                                decimal.Parse(productsTable.Cells[row, 5].Text.Trim()), // выход продукта
                                decimal.Parse(productsTable.Cells[4, col].Text.Trim()) // K брутто j
                            };
                        indexAndValue[colText] = values;
                    }
                }

                if (indexAndValue.Count > 0)
                {
                    products.Add(rowIndex, indexAndValue);
                }
            }
        }
        globalVar.Value += 1;
    }

    private void InitializeChemistryTable()
    {
        ExcelWorksheet chemicalTable = new ExcelPackage(new FileInfo(@"Chemistry.xlsx"))
            .Workbook.Worksheets["Лист1"];

        int rowCount = chemicalTable.Dimension.Rows;
        int colCount = chemicalTable.Dimension.Columns;

        for (int col = 3; col < colCount; col++)
        {
            chemicalTableStructure.Add(chemicalTable.Cells[1, col].Text.Trim());
        }

        Parallel.For(2, rowCount + 1, row =>
        {
            var values = new List<decimal>(colCount - 2);

            for (int col = 3; col <= colCount; col++)
            {
                var cellText = chemicalTable.Cells[row, col].Text.Trim();
                if (decimal.TryParse(cellText, out decimal value))
                {
                    values.Add(value);
                }
            }

            int key = int.Parse(chemicalTable.Cells[row, 1].Text.Trim());
            chemical[key] = values;
        });
        globalVar.Value += 1;
    }
}
