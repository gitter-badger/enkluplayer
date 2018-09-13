using UnityEngine;

namespace CreateAR.EnkluPlayer.Test
{
    public class DummyPoolManager : IAssetPoolManager
    {
        public T Get<T>(GameObject prefab) where T : class
        {
            return Object.Instantiate(prefab) as T;
        }

        public bool Put(GameObject gameObject)
        {
            Object.Destroy(gameObject);

            return true;
        }
    }
}