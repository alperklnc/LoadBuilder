using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LoadBuilder.Packing;
using LoadBuilder.Packing.Algorithms;
using LoadBuilder.Packing.Entities;
using OfficeOpenXml;

namespace LoadBuilder
{
    public static class Program
    {
        private static string _mainPath;
        
        private static readonly List<Container> Containers = new();
        private static readonly Dictionary<string, Item> Items = new();

        public static void Main(string[] args)
        {
            _mainPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.ToString();
            
            ReadDataFiles();
            Solve();
        }

        private static void ReadDataFiles()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ReadContainerFile();
            ReadItemFile();
        }
        
        private static void ReadContainerFile()
        {
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/container_dimensions.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();
                
                Container container = new Container(i - 1, row[0], decimal.Parse(row[1]), decimal.Parse(row[2]), decimal.Parse(row[3]));
                Containers.Add(container);
            }
        }
        
        private static void ReadItemFile()
        {
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/Book1.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 5; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();

                if (row.Count == 9)
                {
                    var id = row[0];
                    var type = row[3];
                    var length = decimal.Parse(row[6]);
                    var width = decimal.Parse(row[7]);
                    var height = decimal.Parse(row[8]);

                    Item item = new Item(i, type, length, width, height);
                    if (!Items.TryGetValue(id, out var _item))
                    {
                        Items.Add(id, item);
                    }
                }
                else if (row.Count == 8)
                {
                    var id = row[0];
                    var type = row[2];
                    var length = decimal.Parse(row[5]);
                    var width = decimal.Parse(row[6]);
                    var height = decimal.Parse(row[7]);

                    Item item = new Item(i, type, length, width, height);
                    if (!Items.TryGetValue(id, out var _item))
                    {
                        Items.Add(id, item);
                    }
                }
            }
        }

        private static void Solve()
        {
            var container = Containers[2];

            var itemsToPack = new List<Item>();
            var itemTypeCount = 1;
            var totalItemAmount = 0;
            
            for (int i = 0; i < itemTypeCount; i++)
            {
                var refrigeratorAmount = 7;

                if (Items.TryGetValue("8990461600", out var refrigerator))
                {
                    refrigerator.IsFullRotationAllowed = false;
                    refrigerator.Quantity = refrigeratorAmount;
                    
                    itemsToPack.Add(refrigerator);
                    totalItemAmount += refrigeratorAmount;
                }
                
                Console.WriteLine("MISSING ITEM ID!!!");
            }
            
            var packingResults = PackingService.Pack(container, itemsToPack, new List<int> { (int)AlgorithmType.EB_AFIT });

            Console.WriteLine("==================== PACKING RESULT ====================");
            Console.WriteLine($"{totalItemAmount} items with {itemsToPack.Count} different types are packed into {packingResults.Count} Container(s)\n");
            
            foreach (var result in packingResults)
            {
                result.PrintResults(true);

                var path = "/Users/alperkilinc/Desktop/KU/INDR491/LoadBuilder/LoadBuilder/Output";
                var fileName = $"output_{result.AlgorithmPackingResults[0].AlgorithmName}";
                result.WriteResultsToTxt(path, fileName);
                
                VisualizeOutput(path, fileName);
            }
        }
        
        private static void VisualizeOutput(string path, string fileName)
        {
            Process p = new Process();
            p.StartInfo.FileName = "python3";
            p.StartInfo.Arguments = $"visualizer.py {path} {fileName}";

            p.StartInfo.RedirectStandardInput = true;
            
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.WorkingDirectory = $"{_mainPath}/Helpers";
            p.StartInfo.UseShellExecute = false;

            p.Start();
            Console.WriteLine("Process StandardOutput");
            Console.Write(p.StandardOutput.ReadToEnd());
            Console.WriteLine("Process StandardError");
            Console.Write(p.StandardError.ReadToEnd());
            p.WaitForExit();
            p.Close();
        }
    }
}