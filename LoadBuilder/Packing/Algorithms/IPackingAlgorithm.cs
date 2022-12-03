using System.Collections.Generic;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Packing.Algorithms
{
    public interface IPackingAlgorithm
    {
        AlgorithmPackingResult Run(Container container, List<Item> items);
    }
}