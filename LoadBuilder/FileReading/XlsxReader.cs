using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LoadBuilder.Packing.Entities;
using OfficeOpenXml;

namespace LoadBuilder.FileReading
{
    public class XlsxReader
    {
        private static string _mainPath;
        
        public XlsxReader(string mainPath)
        {
            _mainPath = mainPath;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public void ReadContainerFile(out List<Container> containers)
        {
            containers = new List<Container>();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/container_dimensions.xlsx"));
            var myWorksheet = xlPackage.Workbook.Worksheets.First();
            var totalRows = myWorksheet.Dimension.End.Row;
            var totalColumns = myWorksheet.Dimension.End.Column;

            for (int i = 2; i <= totalRows; i++)
            {
                var row = myWorksheet.Cells[i, 1, i, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToList();
                
                Container container = new Container(i - 1, row[0], decimal.Parse(row[1]), decimal.Parse(row[2]), decimal.Parse(row[3]));
                containers.Add(container);
            }
        }
        
        public void ReadItemFile(out Dictionary<string, Item> items)
        {
            items = new Dictionary<string, Item>();
            
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

                    if (!items.ContainsKey(id))
                    {
                        items.Add(id, item);
                    }
                    else
                    {
                        Console.WriteLine($"Item {id} is already added to the dictionary!");
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

                    if (!items.ContainsKey(id))
                    {
                        items.Add(id, item);
                    }
                    else
                    {
                        Console.WriteLine($"Item {id} is already added to the dictionary!");
                    }
                }
            }
        }
        
        public void ReadLoadingTypesFile(out Dictionary<string, Dictionary<string, string>> loadingTypes)
        {
            loadingTypes = new Dictionary<string, Dictionary<string, string>>();
            
            using ExcelPackage xlPackage = new ExcelPackage(new FileInfo($"{_mainPath}/Data/Loading Types.xlsx"));
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
        }
    }
}