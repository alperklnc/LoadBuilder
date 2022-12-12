namespace LoadBuilder.Packing.Entities
{
    public static class LoadingType
    {
        public const string FullLoading = "Full Loading";
        public const string UnloadingWithClamp = "Unloading with Clamp";
        public const string WithoutHorizontal = "Without Horizontal";
        
        public static bool IsFullLoading(string loadingType)
        {
            return loadingType == FullLoading;
        }
        
        public static bool IsUnloadingWithClamp(string loadingType)
        {
            return loadingType == UnloadingWithClamp;
        }
        
        public static bool IsWithoutHorizontal(string loadingType)
        {
            return loadingType == WithoutHorizontal;
        }
    }
}
