using System.Collections.Generic;
using System.Diagnostics;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Packing.Algorithms
{
    public interface IPackingAlgorithm
    {
        AlgorithmPackingResult Run(string orderDocumentNumber, Container container, List<Item> items,
            bool unloadingWithClamp, Stopwatch stopwatch);
    }
}