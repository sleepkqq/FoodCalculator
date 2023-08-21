using System.Collections.Concurrent;
using Testing.Models;

namespace Testing.Repositories;

public interface IProductResultRepository
{
    Task InitProductResultTable(ConcurrentQueue<ProductResult> productResult);
    Task ManageProductResultTable();
}
