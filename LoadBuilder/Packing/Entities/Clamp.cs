namespace LoadBuilder.Packing.Entities
{
    public class Clamp
    {
        public int MaxOpeningLength { get; set; }
        public int ArmWidth { get; set; }
        public int ArmHeight { get; set; }

        public int ArmThickness = 10;

        public int BottomOffset = 30;

        public Clamp(int maxOpeningLength, int armWidth, int armHeight)
        {
            MaxOpeningLength = maxOpeningLength;
            ArmWidth = armWidth;
            ArmHeight = armHeight;
        }
    }

    public static class ClampTypes
    {
        public static Clamp Clamp1 = new Clamp(220,150,150);
        public static Clamp Clamp2 = new Clamp(240,160,160);
    }
}