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

        public Item Bar { get; set; }

        public Container(int id, string type, decimal length, decimal width, decimal height)
        {
            ID = id;
            Type = type;
            Length = length;
            Width = width;
            Height = height;
            Volume = length * width * height;

            if (HasBar())
            {
                Bar = CreateContainerBar(50, 80);
            }
        }

        public bool HasBar()
        {
            return false;
            return Type != "Truck";
        }

        private Item CreateContainerBar(int barWidth, int barHeight)
        {
            var bar = new Item(-1, "Bar", Length, barWidth, barHeight, RotationType.OnlyDefault,1)
            {
                IsPacked = true,
                CoordX = 0,
                CoordY = Width - barWidth,
                CoordZ = Height - barHeight,
                PackDimX = Length,
                PackDimY = barWidth,
                PackDimZ = barHeight
            };
            return bar;
        }
    }
}