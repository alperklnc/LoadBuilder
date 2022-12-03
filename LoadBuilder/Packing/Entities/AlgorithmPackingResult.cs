using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LoadBuilder.Packing.Entities
{
    public class AlgorithmPackingResult
    {
        public int AlgorithmId { get; set; }

        public string AlgorithmName { get; set; }
        
        public bool IsCompletePack { get; set; }
        
        public List<Item> PackedItems { get; set; }
        
        public long PackTimeInMilliseconds { get; set; }
        
        public decimal PercentContainerVolumePacked { get; set; }

        public decimal PercentItemVolumePacked { get; set; }
        
        public List<Item> UnpackedItems { get; set; }
        
        public AlgorithmPackingResult()
        {
            PackedItems = new List<Item>();
            UnpackedItems = new List<Item>();
        }
    }
}