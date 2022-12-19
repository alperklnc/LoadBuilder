using System.Collections.Generic;

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