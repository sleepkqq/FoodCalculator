using System.Collections;
using System.Collections.Concurrent;
using Testing.Data;
using Testing.Models;
using Testing.Repositories;

namespace Testing.Services;

public class DatabaseWithExcelTablesService
{
    private readonly GlobalVariable globalVar = GlobalVariable.Instance;
    private readonly ExcelInitializer excelInitializer = new();
    private readonly ConcurrentQueue<ChemicalResult> chemicalResult = new();
    private readonly ConcurrentQueue<ProductResult> productResult = new();
    private readonly Hashtable products;
    private readonly Hashtable chemical;
    private readonly string connectionString = new ConnectionString().connectionString;
    private readonly IProductResultRepository productResultRepository;
    private readonly IChemicalResultRepository chemicalResultRepository;

    public DatabaseWithExcelTablesService()
    {
        products = excelInitializer.products;
        chemical = excelInitializer.chemical;
        productResultRepository = new SqlProductResult(connectionString, excelInitializer.productTableStructure);
        chemicalResultRepository = new SqlChemicalResult(connectionString, excelInitializer.chemicalTableStructure);
    }

    public async Task DiariesInfoAsync()
    {

        Task initExcel = Task.Run(() => excelInitializer.InitializeTables());
        Task<List<ChildDiary>> getChildDiariesTask = Task.Run(() => new Rp2023StudentRandContext().ChildDiaries.ToList());
        Task<List<AdultDiary>> getAdultDiariesTask = Task.Run(() => new Rp2023StudentRandContext().AdultDiaries.ToList());

        await Task.WhenAll(initExcel, getChildDiariesTask, getAdultDiariesTask);

        globalVar.Value += 2;

        Task manageChemical = Task.Run(() => chemicalResultRepository.ManageChemicalResultTable());
        Task manageProduct = Task.Run(() => productResultRepository.ManageProductResultTable());
        Task processAdult = Task.Run(() => ProcessDiariesAsync(getAdultDiariesTask.Result));
        Task processChild = Task.Run(() => ProcessDiariesAsync(getChildDiariesTask.Result));

        await Task.WhenAll(manageChemical, manageProduct, processChild, processAdult);

        Task initProductDb = Task.Run(() => InitProductDb());
        Task initChemicalDb = Task.Run(() => InitChemicalDb());

        await Task.WhenAll(initChemicalDb, initProductDb);
    }

    private async Task InitProductDb()
    {
        int batchSize = 150000;
        List<Task> tasks = new();

        for (int i = 0; i < productResult.Count; i += batchSize)
        {
            var batch = new ConcurrentQueue<ProductResult>(productResult.Skip(i).Take(batchSize));

            tasks.Add(new SqlProductResult(connectionString, excelInitializer.productTableStructure).InitProductResultTable(batch));
        }
        await Task.WhenAll(tasks);
        globalVar.Value += 1;
    }

    private async Task InitChemicalDb()
    {
        int batchSize = 150000;
        List<Task> tasks = new();

        for (int i = 0; i < chemicalResult.Count; i += batchSize)
        {
            var batch = new ConcurrentQueue<ChemicalResult>(chemicalResult.Skip(i).Take(batchSize));

            tasks.Add(new SqlChemicalResult(connectionString, excelInitializer.chemicalTableStructure).InitChemicalResultTable(batch));
        }
        await Task.WhenAll(tasks);
        globalVar.Value += 1;
    }

    private async Task ProcessDiariesAsync<T>(List<T> diaries)
    {
        var tasks = diaries.Select(async (diary) => {
            int productId = 0;
            int weight = 0;
            Guid objectId = Guid.NewGuid();

            if (diary is AdultDiary adultDiary)
            {
                productId = (int)adultDiary.IR6v66;
                weight = (int)adultDiary.IR6v65;
                objectId = adultDiary.ObjectId;
            }
            else if (diary is ChildDiary childDiary)
            {
                productId = (int)childDiary.CR6v46;
                weight = (int)childDiary.CR6v45;
                objectId = childDiary.ObjectId;
            }

            if (products.ContainsKey(productId))
            {
                Hashtable productValues = new();

                foreach (DictionaryEntry entry in (Hashtable)products[productId])
                {
                    productValues[entry.Key] = ProductFormula(weight, (List<decimal>)entry.Value);
                }

                if (chemical.ContainsKey(productId))
                {
                    List<decimal> chemicalValues = new();

                    foreach (var n in (List<decimal>)chemical[productId])
                    {
                        chemicalValues.Add(ChimecalFormula(weight, n));
                    }

                    chemicalResult.Enqueue(new ChemicalResult(objectId, productId, chemicalValues));
                }
                productResult.Enqueue(new ProductResult(objectId, productId, productValues));
            }
        });

        await Task.WhenAll(tasks);
        globalVar.Value += 1;
    }

    private static decimal ChimecalFormula(int weight, decimal nutrientValue)
    {
        return weight * nutrientValue / 100;
    }

    private static decimal ProductFormula(int weight, List<decimal> values)
    {//                  n (j, x);  выход продукта;            K брутто j
        return (weight * values[0] / values[1]) * 100 / (100 - values[2]);
    }
}
