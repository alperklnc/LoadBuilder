using System.Collections.Generic;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Packing.Algorithms
{
    public abstract class AlgorithmBase
    {
        public abstract ContainerPackingResult Run(Container container, List<Item> items);
    }
}