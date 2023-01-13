using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LoadBuilder.Orders;
using LoadBuilder.Packing.Entities;
using OfficeOpenXml;

namespace LoadBuilder.FileReading
{
    public class XlsxReader
    {
        readonly Stopwatch _stopwatch = new();
        
        private static string _mainPath;
        
        public XlsxReader(string mainPath)
        {
            _mainPath = mainPath;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public void ReadContainerFile(out Dictionary<string, Container> containerDict)
        {
            containerDict = new Dictionary<string, Container>();
            
            Console.WriteLine("Reading container_dimensions.xlsx file.");
            _stopwatch.Restart();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/container_dimensions.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();
                
                Container container = new Container(i - 1, row[0], decimal.Parse(row[1]), decimal.Parse(row[2]), decimal.Parse(row[3]));
                containerDict.Add(container.Type, container);
            }
            
            _stopwatch.Stop();
            Console.WriteLine($"Finished in {_stopwatch.ElapsedMilliseconds} ms\n");
        }
        
        public void ReadItemFile(out Dictionary<string, Item> items)
        {
            items = new Dictionary<string, Item>();
            
            Console.WriteLine("Reading Book1-trimmed.xlsx file.");
            _stopwatch.Restart();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/Book1-trimmed.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();

                if (row.Count < 5)
                {
                    continue;
                }
                
                var id = row[0];
                var type = row[1];
                var length = decimal.Parse(row[2]);
                var width = decimal.Parse(row[3]);
                var height = decimal.Parse(row[4]);

                Item item = new Item(i, id, type, length, width, height);

                if (!items.ContainsKey(id))
                {
                    items.Add(id, item);
                }
                else
                {
                    //Console.WriteLine($"Item {id} is already added to the dictionary!");
                }
            }
            
            _stopwatch.Stop();
            Console.WriteLine($"Finished in {_stopwatch.ElapsedMilliseconds} ms\n");
        }
        
        public void ReadLoadingTypesFile(out Dictionary<string, Dictionary<string, string>> loadingTypes)
        {
            loadingTypes = new Dictionary<string, Dictionary<string, string>>();
            
            Console.WriteLine("Reading Loading Types-tr.xlsx file.");
            _stopwatch.Restart();

            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/Loading Types-tr.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            var items = myWorksheet.Cells[1, 1, 1, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();
            
            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();

                var loadingTypesForCountry = new Dictionary<string, string>();

                var country = row[0];
                for (int j = 1; j < row.Count; j++)
                {
                    loadingTypesForCountry.Add(items[j], row[j]);
                }
                loadingTypes.Add(country, loadingTypesForCountry);
            }
            
            _stopwatch.Stop();
            Console.WriteLine($"Finished in {_stopwatch.ElapsedMilliseconds} ms\n");
        }
        
        public void ReadTmFile(out Dictionary<string, OrderDetails> orderDetails)
        {
            orderDetails = new Dictionary<string, OrderDetails>();
            
            Console.WriteLine("Reading TM Yükleme Takip Raporu-trimmed.xlsx file.");
            _stopwatch.Restart();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/TM Yükleme Takip Raporu-trimmed.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();

                if (row.Count < 4)
                {
                    continue;
                }
                
                var orderId = row[0];
                var containerType = GetContainerType(row[1]);
                var country = row[3];

                if (!string.IsNullOrEmpty(orderId))
                {
                    var orderDetail = new OrderDetails(containerType, country);

                    if (!orderDetails.ContainsKey(orderId))
                    {
                        orderDetails.Add(orderId, orderDetail);
                    }
                    else
                    {
                        //Console.WriteLine($"Order {orderId} has already added to the dictionary.");
                    }
                }
            }
            
            _stopwatch.Stop();
            Console.WriteLine($"Finished in {_stopwatch.ElapsedMilliseconds} ms\n");
        }

        private string GetContainerType(string containerInfo)
        {
            if (containerInfo.Contains("20DC"))
            {
                return "20DC";
            } 
            if (containerInfo.Contains("40DC"))
            {
                return "40DC";
            } 
            if (containerInfo.Contains("40HC"))
            {
                return "40HC";
            } 
            if (containerInfo.Contains("45HC"))
            {
                return "45HC";
            } 
            if (containerInfo.Contains("Truck"))
            {
                return "Truck";
            }

            return string.Empty;
        }
        
        public void ReadOrderFile(Dictionary<string, OrderDetails> orderDetails, out Dictionary<string, OrderInfo> previousOrders, out Dictionary<string, OrderInfo> newOrders)
        {
            previousOrders = new Dictionary<string, OrderInfo>();
            newOrders = new Dictionary<string, OrderInfo>();
            
            Console.WriteLine("Reading siparişler.xlsx file.");
            _stopwatch.Restart();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/siparişler.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();

                if (row[0] == String.Empty)
                {
                    break;
                }
                
                var documentNumber = row[4];
                var customerId = row[5];
                var materialId = row[11];
                var loadedQuantity = int.Parse(row[19]);
                var orderQuantity = int.Parse(row[20]);

                if (loadedQuantity > 0)
                {
                    if (orderDetails.TryGetValue(documentNumber, out var orderDetail))
                    {
                        var country = orderDetail.Country;
                        var containerType = orderDetail.ContainerType;
                        
                        AddOrder(previousOrders, documentNumber, materialId, orderQuantity, customerId, country, true, containerType);
                    }
                    else
                    {
                        //Console.WriteLine($"Order {documentNumber} does not exist in the TM Report");
                    }
                }
                else
                {
                    AddOrder(newOrders, documentNumber, materialId, orderQuantity, customerId);
                }
            }
            
            _stopwatch.Stop();
            Console.WriteLine($"Finished in {_stopwatch.ElapsedMilliseconds} ms\n");
        }

        private static void AddOrder(Dictionary<string, OrderInfo> previousOrders, string documentNumber,
            string materialId, int orderQuantity,
            string customerId, string country = "", bool isShipped = false, string containerType = "")
        {
            if (previousOrders.TryGetValue(documentNumber, out var order))
            {
                order.AddItem(materialId, orderQuantity);
            }
            else
            {
                order = new OrderInfo(documentNumber, customerId, country, isShipped, containerType);
                order.AddItem(materialId, orderQuantity);

                previousOrders.Add(documentNumber, order);
            }
        }
    }
}