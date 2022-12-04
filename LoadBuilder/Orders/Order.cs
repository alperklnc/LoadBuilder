using System.Collections.Generic;

namespace LoadBuilder.Orders
{
    public class OrderInfo
    {
        public readonly string Country;
        public readonly Dictionary<string, int> Items = new();

        public OrderInfo()
        {
            Country = "";
        }

        public void AddItem(string itemId, int itemAmount)
        {
            Items.Add(itemId, itemAmount);
        }
    }
}