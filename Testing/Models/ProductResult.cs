using System.Collections;

namespace Testing.Models;

public class ProductResult
{
    public Guid ObjectId { get; set; }
    public int? ProductId { get; set; }
    public Hashtable? ProductValues { get; set; }

    public ProductResult(Guid objectId, int productId, Hashtable? productValues)
    {
        ObjectId = objectId;
        ProductId = productId;
        ProductValues = productValues;
    }
}
