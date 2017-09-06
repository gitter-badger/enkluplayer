namespace CreateAR.SpirePlayer
{
    public class AssetManagerConfiguration
    {
        public IAssetLoader Loader;
        public IQueryResolver Queries;
        public IAssetUpdateService Service;

        public bool IsValid()
        {
            return null != Loader && null != Queries;
        }
    }
}