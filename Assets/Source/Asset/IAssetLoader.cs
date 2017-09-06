using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IAssetLoader
    {
        IAsyncToken<Object> Load(AssetInfo info);
    }
}