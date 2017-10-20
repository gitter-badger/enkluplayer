using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IAssetPoolManager
    {
        T Get<T>(GameObject prefab) where T : class;
        bool Put(GameObject gameObject);
    }
}