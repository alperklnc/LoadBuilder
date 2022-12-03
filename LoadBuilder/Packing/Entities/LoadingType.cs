namespace LoadBuilder.Packing.Entities
{
    public static class LoadingType
    {
        private const string FullLoading = "Full Loading";
        private const string UnloadingWithClamp = "Unloading with Clamp";
        private const string WithoutHorizontal = "Without Horizontal";
        
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
