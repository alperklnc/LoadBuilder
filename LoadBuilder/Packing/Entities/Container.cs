namespace LoadBuilder.Packing.Entities
{
    public class Container
    {
        public int ID { get; set; }

        public string Type { get; set; }

        public decimal Length { get; set; }
        
        public decimal Width { get; set; }
        
        public decimal Height { get; set; }

        public decimal Volume { get; set; }

        public Container(int id, string type, decimal length, decimal width, decimal height)
        {
            ID = id;
            Type = type;
            Length = length;
            Width = width;
            Height = height;
            Volume = length * width * height;
        }
    }
}