

namespace Testing.Models;

public class ChemicalResult
{
    public Guid ObjectId { get; set; }
    public int? ProductId { get; set; }
    public List<decimal>? ChemicalValues { get; set; }


    public ChemicalResult(Guid objectId, int productId, List<decimal>? chemicalValues)
    {
        ObjectId = objectId;
        ProductId = productId;
        ChemicalValues = chemicalValues;
    }
}
