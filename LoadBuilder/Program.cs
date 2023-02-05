using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static readonly Stopwatch Stopwatch = new();
        
        public static void Main(string[] args)
        {
            ReadData();
            
            // _mainPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.ToString();
            // Visualizer.VisualizeOutput(_mainPath, "volume_4");
            // return;
            
            //var order = _previousOrders["5600162932"];
            var order = _previousOrders["5600164176"];

            order.WriteOrderToTxt($"{_mainPath}/Output", "order", _containers[order.ContainerType], _items, _loadingTypes);
            SolveByAdaptive(order);
            //SolveByBestfit(order);
        }
        
        private static void SolveOrder(OrderInfo order)
        {
            if (OrderInfo.IsOrderValid(order))
            {
                var algorithm = AlgorithmHelper.FindAlgorithm(_items, _loadingTypes, order);

                switch (algorithm)
                {
                    case AlgorithmType.adaptiveheuristic:
                        SolveByAdaptive(order);
                        break;
                    case AlgorithmType.bestfit:
                        SolveByBestfit(order);
                        break;
                }
            }
        }

        private static void ReadData()
        {
            _mainPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.ToString();
            
            Stopwatch.Restart();
            
            XlsxReader xlsxReader = new XlsxReader(_mainPath);
            xlsxReader.ReadContainerFile(out _containers);
            xlsxReader.ReadItemFile(out _items);
            xlsxReader.ReadLoadingTypesFile(out _loadingTypes);
            xlsxReader.ReadTmFile(out _orderDetails);
            xlsxReader.ReadOrderFile(_orderDetails, out _previousOrders, out _newOrders);
            
            Console.WriteLine($"Total time to read data: {Stopwatch.ElapsedMilliseconds} ms");
        }

        private static void SolveByAdaptive(OrderInfo order, int counter = 1)
        {
            Stopwatch.Restart();
            
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
                //result.PrintResults(false);

                Dictionary<string, int> possibleItemAmounts = null;
                
                if (resultCounter == packingResults.Count)
                {
                    possibleItemAmounts = result.CalculatePossibleItemAmountsForRemainingContainerVolume(container);
                }

                var fileName = $"{counter}_output_{order.DocumentNumber}_{resultCounter}_{result.AlgorithmPackingResults[0].AlgorithmName}";
                result.WriteResultsToTxt($"{_mainPath}/Output", fileName, result.AlgorithmPackingResults[0], order, container, possibleItemAmounts);
                
                Console.WriteLine($"Total time to solve problem with Adaptive Heuristic: {Stopwatch.ElapsedMilliseconds} ms");
                
                Stopwatch.Restart();
                
                Visualizer.VisualizeOutput(_mainPath, fileName);

                resultCounter++;
            }
        }
        
        private static void SolveByBestfit(OrderInfo order)
        {
            order.WriteOrderToTxt($"{_mainPath}/Output", "order", _containers[order.ContainerType], _items, _loadingTypes);
            
            Stopwatch.Restart();
            AlgorithmHelper.RunMethod("bestfit", _mainPath, "order.txt");
            Console.WriteLine($"Total time to solve problem with Best Fit: {Stopwatch.ElapsedMilliseconds} ms");
            
            Stopwatch.Restart();
            Visualizer.VisualizeOutput(_mainPath, "bestfit_output");
        }
        
        private static OrderInfo CreateDummyOrder()
        {
            var dummyOrder = new OrderInfo();
            dummyOrder.DocumentNumber = "5600157081";
            dummyOrder.Country = "Rusya";
            
            dummyOrder.ContainerType = "40HC";
            dummyOrder.AddItem("6202815000", 7);
            dummyOrder.AddItem("6200702000", 2);

            return dummyOrder;
        }
        
        private static void SolveAllOrders()
        {
            var counter = 1;
            foreach (var _order in _previousOrders)
            {
                Console.WriteLine($"{counter} | Solving {_order.Key}");
                SolveByAdaptive(_order.Value, counter);
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
    }
}