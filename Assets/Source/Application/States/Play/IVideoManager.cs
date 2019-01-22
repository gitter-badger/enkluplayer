using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    public interface IVideoManager
    {
        void EnableUploads(string tag);
        void DisableUploads();
    }
}