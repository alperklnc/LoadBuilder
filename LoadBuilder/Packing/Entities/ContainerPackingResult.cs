using System;
using System.Collections.Generic;
using System.IO;

namespace LoadBuilder.Packing.Entities
{
    public class ContainerPackingResult
    {
        public int ContainerId { get; set; }

        public int ContainerNumber { get; set; }

        public List<AlgorithmPackingResult> AlgorithmPackingResults { get; set; }
        public ContainerPackingResult(int containerNumber)
        {
            AlgorithmPackingResults = new List<AlgorithmPackingResult>();
            ContainerNumber = containerNumber;
        }

        public void PrintResults(bool showItems)
        {
            Console.WriteLine($"\n========== CONTAINER {ContainerNumber} ==========");
            Console.WriteLine($"===== Total Item Amount: {AlgorithmPackingResults[0].PackedItems.Count + AlgorithmPackingResults[0].UnpackedItems.Count}");
            Console.WriteLine($"===== Packed Item Amount: {AlgorithmPackingResults[0].PackedItems.Count}");
            Console.WriteLine($"===== Unpacked Item Amount: {AlgorithmPackingResults[0].UnpackedItems.Count}");
            Console.WriteLine($"===== Item Packing Percentage: %{AlgorithmPackingResults[0].PercentItemVolumePacked}");
            Console.WriteLine($"===== Container Utilization: %{AlgorithmPackingResults[0].PercentContainerVolumePacked}");
            Console.WriteLine($"===== Solution Time: {AlgorithmPackingResults[0].PackTimeInMilliseconds}ms");


            if (showItems)
            {
                Console.WriteLine("\n========== PACKED ITEMS ==========");
                foreach (var packedItem in AlgorithmPackingResults[0].PackedItems)
                {
                    Console.WriteLine($"=== {packedItem.Type}({packedItem.Dim1},{packedItem.Dim2},{packedItem.Dim3}) - " +
                                      $"Position: ({packedItem.CoordX},{packedItem.CoordY},{packedItem.CoordZ}) - " +
                                      $"Rotation Type: Type {packedItem.RotationType}");
                }
            
                Console.WriteLine("\n========== UNPACKED ITEMS ==========");
                foreach (var unpackedItem in AlgorithmPackingResults[0].UnpackedItems)
                {
                    Console.WriteLine($"=== {unpackedItem.Type}({unpackedItem.Dim1},{unpackedItem.Dim2},{unpackedItem.Dim3})");
                }
            }
        }
        
        public void WriteResultsToTxt(string path, string fileName)
        {
            using (StreamWriter writer = new StreamWriter($"{path}/{fileName}.txt"))  
            {
                foreach (var packedItem in AlgorithmPackingResults[0].PackedItems)
                {
                    var line = $"{packedItem.CoordX} {packedItem.CoordY} {packedItem.CoordZ} {packedItem.Dim1} {packedItem.Dim2} {packedItem.Dim3}";
                    writer.WriteLine(line); 
                }
            }
        }
    }
}