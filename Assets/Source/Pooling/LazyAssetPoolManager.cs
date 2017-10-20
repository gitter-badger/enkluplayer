using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class LazyAssetPoolManager : IAssetPoolManager
    {
        public T Get<T>(GameObject prefab) where T : class
        {
            return Cast<T>(Object.Instantiate(prefab));
        }

        public bool Put(GameObject gameObject)
        {
            Object.Destroy(gameObject);

            return true;
        }

        private static T Cast<T>(GameObject gameObject) where T : class
        {
            var cast = gameObject as T;
            if (null == cast)
            {
                if (typeof(Component) == typeof(T))
                {
                    var component = gameObject.GetComponent<T>()
                                    ?? gameObject.GetComponentInChildren<T>();
                    if (null == component)
                    {
                        Object.Destroy(gameObject);
                    }

                    return component;
                }

                Object.Destroy(gameObject);

                return null;
            }

            return cast;
        }
    }
}