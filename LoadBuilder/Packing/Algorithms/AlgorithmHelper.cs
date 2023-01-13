using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LoadBuilder.Orders;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Packing.Algorithms
{
    public class AlgorithmHelper
    {
        public static AlgorithmType FindAlgorithm(Dictionary<string, Item> items, Dictionary<string, Dictionary<string, string>> loadingTypes, OrderInfo order)
        {
            var itemTypes = new List<string>();
            
            foreach (var orderedItem in order.OrderedItems)
            {
                if (!items.TryGetValue(orderedItem.Key, out var item)) continue;
                
                itemTypes.Add(item.Type);
                    
                var loadingType = loadingTypes[order.Country][item.Type];
                if (LoadingType.IsUnloadingWithClamp(loadingType))
                {
                    return AlgorithmType.adaptiveheuristic;
                }
            }

            if (order.OrderedItems.Count < 4)
            {
                return itemTypes.All(n => n == itemTypes[0]) ? AlgorithmType.bestfit : AlgorithmType.bestfit;
            }
            
            return AlgorithmType.adaptiveheuristic;
        }
        
        public static void RunMethod(string method, string path, string fileName)
        {
            Process p = new Process();
            p.StartInfo.FileName = "python3";
            p.StartInfo.Arguments = $"{method}.py {path}/Output {fileName}";

            p.StartInfo.RedirectStandardInput = true;
            
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.WorkingDirectory = $"{path}/Packing/Algorithms";
            p.StartInfo.UseShellExecute = false;

            p.Start();
            //Console.WriteLine("Process StandardOutput");
            Console.Write(p.StandardOutput.ReadToEnd());
            //Console.WriteLine("Process StandardError");
            Console.Write(p.StandardError.ReadToEnd());
            p.WaitForExit();
            p.Close();
        }
    }
}