using System.Collections.Concurrent;
using Testing.Models;

namespace Testing.Repositories;

public interface IChemicalResultRepository
{
    Task InitChemicalResultTable(ConcurrentQueue<ChemicalResult> chemicalResult);
    Task ManageChemicalResultTable();
}
