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
        
        public static List<string> TimeOutOrders = new();

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

            order.WriteOrderToTxt($"{_mainPath}/Output", "order", _containers[order.ContainerType], _items, _loadingTypes);

            if (OrderInfo.IsOrderValid(order))
            {
                var algorithm = AlgorithmHelper.FindAlgorithm(_items, _loadingTypes, order);

                if (algorithm == AlgorithmType.adaptiveheuristic)
                {
                    Solve(order);
                }
                else if (algorithm == AlgorithmType.bestfit)
                {
                    AlgorithmHelper.RunMethod("bestfit", _mainPath, "order.txt");
                    Visualizer.VisualizeOutput(_mainPath, "bestfit_output");
                }
            }
        }

        private static OrderInfo CreateDummyOrder()
        {
            var dummyOrder = new OrderInfo();
            dummyOrder.DocumentNumber = "DUMMY";
            dummyOrder.Country = "Danimarka";
            
            //dummyOrder.Country = "ABD";
            dummyOrder.ContainerType = "40HC";
            dummyOrder.AddItem("4410900046", 27); //65
            dummyOrder.AddItem("6203202000", 27); //65
            //dummyOrder.AddItem("6203202000", 27); //65
            //dummyOrder.AddItem("7305530099", 36); //45
            
            // dummyOrder.AddItem("7278440517", 27);
            // dummyOrder.AddItem("7293442884", 1);
            // dummyOrder.AddItem("7248846912", 31);
            // dummyOrder.AddItem("7298547681", 1);

            return dummyOrder;
        }

        private static void SolveAllOrders()
        {
            var counter = 1;
            foreach (var _order in _previousOrders)
            {
                Console.WriteLine($"{counter} | Solving {_order.Key}");
                Solve(_order.Value, counter);
                counter++;
            }
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

        private static void Solve(OrderInfo order, int counter = 1)
        {
            var itemsToPack = new List<Item>();
            var totalItemAmount = 0;
            var unloadingWithClamp = false;

            foreach (var orderedItem in order.OrderedItems)
            {
                if (_items.TryGetValue(orderedItem.Key, out var item))
                {
                    item.Quantity = orderedItem.Value;
                    var loadingType = _loadingTypes[order.Country][item.Type];
                    item.LoadingType = loadingType;
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
            
            var packingResults = PackingService.Pack(order.DocumentNumber, container, itemsToPack, unloadingWithClamp, new List<int> { (int)AlgorithmType.adaptiveheuristic });

            if (packingResults[0].AlgorithmPackingResults.Count == 0)
            {
                return;
            }

            var resultCounter = 1;
            foreach (var result in packingResults)
            {
                result.PrintResults(false);

                var fileName = $"{counter}_output_{order.DocumentNumber}_{resultCounter}_{result.AlgorithmPackingResults[0].AlgorithmName}";
                result.WriteResultsToTxt($"{_mainPath}/Output", fileName, result.AlgorithmPackingResults[0], order, container);
                
                Visualizer.VisualizeOutput(_mainPath, fileName);

                resultCounter++;
            }
        }
    }
}