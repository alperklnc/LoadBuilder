using System;
using System.Collections.Generic;
using System.IO;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Orders
{
    public class OrderInfo
    {
        public string DocumentNumber { get; set; }
        public string CustomerId { get; }
        public string Country { get; set; }
        public bool IsShipped { get; set; }

        public string ContainerType { get; set; }

        public bool IsMixOrder => OrderedItems.Count > 1;

        public readonly Dictionary<string, int> OrderedItems = new();

        public OrderInfo()
        {
        }

        public OrderInfo(string documentNumber, string customerId, string country, bool isShipped = false, string containerType = "")
        {
            DocumentNumber = documentNumber;
            CustomerId = customerId;
            IsShipped = isShipped;
            Country = country;
            ContainerType = containerType;
        }

        public void AddItem(string itemId, int itemAmount)
        {
            if (OrderedItems.ContainsKey(itemId))
            {
                OrderedItems[itemId] += itemAmount;
            }
            else
            {
                OrderedItems.Add(itemId, itemAmount);
            }
        }
        
        public void WriteOrderToTxt(string path, string fileName, Container selectedContainer,
            Dictionary<string, Item> items,
            Dictionary<string, Dictionary<string, string>> loadingTypes)
        {
            using StreamWriter writer = new StreamWriter($"{path}/{fileName}.txt");
            
            var orderInfo = $"{DocumentNumber} {Country} {OrderedItems.Count}";
            writer.WriteLine(orderInfo); 
            
            var containerInfo = $"{ContainerType} {selectedContainer.Length} {selectedContainer.Width} {selectedContainer.Height}";
            writer.WriteLine(containerInfo);

            foreach (var orderedItem in OrderedItems)
            {
                var item = items[orderedItem.Key];
                var rotationType = Item.GetRotationType(loadingTypes[Country][item.Type]);
                
                if (rotationType != null)
                {
                    var line = $"{orderedItem.Key} {orderedItem.Value} {item.Dim1} {item.Dim2} {item.Dim3} {rotationType}";
                    writer.WriteLine(line); 
                }
                else
                {
                    Console.WriteLine("RotationType is missing.");
                }
            }
        }

        public static bool IsOrderValid(OrderInfo order)
        {
            if (string.IsNullOrEmpty(order.Country))
            {
                Console.WriteLine($"Missing country for order {order.DocumentNumber}!");
                return false;
            }
            
            if (string.IsNullOrEmpty(order.ContainerType))
            {
                Console.WriteLine($"Missing ContainerType for order {order.DocumentNumber}!");
                return false;
            }

            return true;
        }
    }

    public class OrderDetails
    {
        public string ContainerType { get; set; }
        public string Country { get; set; }
        
        public OrderDetails(string containerType, string country)
        {
            ContainerType = containerType;
            Country = country;
        }
    }
}