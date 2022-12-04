using System;
using System.Collections.Generic;
using System.IO;
using LoadBuilder.FileReading;
using LoadBuilder.Helpers;
using LoadBuilder.Orders;
using LoadBuilder.Packing;
using LoadBuilder.Packing.Algorithms;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder
{
    public static class Program
    {
        private static string _mainPath;
        
        private static List<Container> _containers = new();
        private static Dictionary<string, Item> _items = new();
        private static Dictionary<string, Dictionary<string, string>> _loadingTypes = new();
        
        private static Container _selectedContainer;

        public static void Main(string[] args)
        {
            _mainPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.ToString();
            
            XlsxReader xlsxReader = new XlsxReader(_mainPath);
            xlsxReader.ReadContainerFile(out _containers);
            xlsxReader.ReadItemFile(out _items);
            xlsxReader.ReadLoadingTypesFile(out _loadingTypes);
            
            OrderInfo order = new OrderInfo();
            order.AddItem("8990461600", 12);

            Solve(order);
        }

        private static void Solve(OrderInfo order)
        {
            _selectedContainer = _containers[2];

            var itemsToPack = new List<Item>();
            var totalItemAmount = 0;

            foreach (var orderedItem in order.Items)
            {
                if (_items.TryGetValue(orderedItem.Key, out var item))
                {
                    if (order.Country == "")
                    {
                        item.RotationType = RotationType.OnlyDefault;
                    }
                    item.Quantity = orderedItem.Value;
                    
                    itemsToPack.Add(item);
                    totalItemAmount += orderedItem.Value;
                }
                else
                {
                    Console.WriteLine($"ITEM ID {orderedItem.Key} is not in the dictionary!");
                }
            }

            var packingResults = PackingService.Pack(_selectedContainer, itemsToPack, new List<int> { (int)AlgorithmType.EB_AFIT });

            Console.WriteLine("==================== PACKING RESULT ====================");
            Console.WriteLine($"{totalItemAmount} items with {itemsToPack.Count} different types are packed into {packingResults.Count} Container(s)\n");
            
            foreach (var result in packingResults)
            {
                result.PrintResults(true);

                var fileName = $"output_{result.AlgorithmPackingResults[0].AlgorithmName}";
                result.WriteResultsToTxt($"{_mainPath}/Output", fileName, _selectedContainer);
                
                Visualizer.VisualizeOutput(_mainPath, fileName);
            }
        }
    }
}