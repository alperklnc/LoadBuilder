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
        
        private static Dictionary<string, Container> _containers = new();
        private static Dictionary<string, Item> _items = new();
        private static Dictionary<string, Dictionary<string, string>> _loadingTypes = new();
        private static Dictionary<string, OrderInfo> _previousOrders = new();
        private static Dictionary<string, OrderInfo> _newOrders = new();
        private static Dictionary<string, OrderDetails> _orderDetails = new();
        
        public static void Main(string[] args)
        {
            _mainPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.ToString();
            
            XlsxReader xlsxReader = new XlsxReader(_mainPath);
            xlsxReader.ReadContainerFile(out _containers);
            xlsxReader.ReadItemFile(out _items);
            xlsxReader.ReadLoadingTypesFile(out _loadingTypes);
            xlsxReader.ReadTmFile(out _orderDetails);
            xlsxReader.ReadOrderFile(_orderDetails, out _previousOrders, out _newOrders);

            var mixedOrders = FindMixOrdersIn(_previousOrders);
            var order = mixedOrders[0];

            var dummyOrder = new OrderInfo();
            dummyOrder.Country = "Danimarka";
            //dummyOrder.Country = "ABD";
            dummyOrder.ContainerType = "40HC";
            dummyOrder.AddItem("6200702000", 27); //65
            //dummyOrder.AddItem("7305530099", 36); //45
            
            
            // dummyOrder.AddItem("7278440517", 27);
            // dummyOrder.AddItem("7293442884", 1);
            // dummyOrder.AddItem("7248846912", 31);
            // dummyOrder.AddItem("7298547681", 1);

            Solve(dummyOrder);
        }

        private static List<OrderInfo> FindMixOrdersIn(Dictionary<string, OrderInfo> previousOrders)
        {
            var previousMixedOrders = new List<OrderInfo>();
            foreach (var order in previousOrders)
            {
                if (order.Value.IsMixOrder)
                {
                    previousMixedOrders.Add(order.Value);
                }
            }

            return previousMixedOrders;
        }

        private static void Solve(OrderInfo order)
        {
            var itemsToPack = new List<Item>();
            var totalItemAmount = 0;
            var unloadingWithClamp = false;

            if (string.IsNullOrEmpty(order.Country))
            {
                Console.WriteLine($"Missing country for order {order.DocumentNumber}!");
                return;
            }
            
            if (string.IsNullOrEmpty(order.ContainerType))
            {
                Console.WriteLine($"Missing ContainerType for order {order.DocumentNumber}!");
                return;
            }

            foreach (var orderedItem in order.OrderedItems)
            {
                if (_items.TryGetValue(orderedItem.Key, out var item))
                {
                    item.Quantity = orderedItem.Value;
                    var loadingType = _loadingTypes[order.Country][item.Type];
                    Console.WriteLine($"Loading Type: {loadingType} for {item.Type} item - ID: {item.ItemId}");
                    item.SetRotationType(loadingType);

                    if (LoadingType.IsUnloadingWithClamp(loadingType))
                    {
                        unloadingWithClamp = true;
                    }
                    
                    itemsToPack.Add(item);
                    totalItemAmount += orderedItem.Value;
                }
                else
                {
                    Console.WriteLine($"ITEM ID {orderedItem.Key} is not in the dictionary!");
                    return;
                }
            }

            var container = _containers[order.ContainerType];
            
            var packingResults = PackingService.Pack(container, itemsToPack, unloadingWithClamp, new List<int> { (int)AlgorithmType.EB_AFIT });

            if (packingResults[0].AlgorithmPackingResults.Count == 0)
            {
                return;
            }
            
            Console.WriteLine("==================== PACKING RESULT ====================");
            Console.WriteLine($"Order Number: {order.DocumentNumber}");
            Console.WriteLine($"Destination: {order.Country}");
            Console.WriteLine($"Container: {order.ContainerType}");
            Console.WriteLine($"Is Already Shipped: {order.IsShipped}");
            foreach (var item in order.OrderedItems)
            {
                Console.WriteLine($"Item ID: {item.Key} - Amount: {item.Value}");
            }
            
            Console.WriteLine($"\n{totalItemAmount} items with {itemsToPack.Count} different types are packed into {packingResults.Count} Container(s)");
            
            foreach (var result in packingResults)
            {
                result.PrintResults(true);

                var fileName = $"output_{result.AlgorithmPackingResults[0].AlgorithmName}";
                result.WriteResultsToTxt($"{_mainPath}/Output", fileName, container);
                
                Visualizer.VisualizeOutput(_mainPath, fileName);
            }
        }
    }
}