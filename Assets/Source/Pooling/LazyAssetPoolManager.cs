using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IAssetPoolManager</c> implementation that does not actually pool.
    /// </summary>
    public class LazyAssetPoolManager : IAssetPoolManager
    {
        /// <inheritdoc cref="IAssetPoolManager"/>
        public T Get<T>(GameObject prefab) where T : class
        {
            return Cast<T>(Object.Instantiate(prefab));
        }

        /// <inheritdoc cref="IAssetPoolManager"/>
        public bool Put(GameObject gameObject)
        {
            Object.Destroy(gameObject);

            return true;
        }

        /// <summary>
        /// Casts a <c>GameObject</c> as a T.
        /// </summary>
        /// <typeparam name="T">The type, generally a component, but maybe <c>GameObject</c>.</typeparam>
        /// <param name="gameObject">The <c>GameObject</c>.</param>
        /// <returns></returns>
        private static T Cast<T>(GameObject gameObject) where T : class
        {
            var cast = gameObject as T;
            if (null == cast)
            {
                var component = gameObject.GetComponent<T>()
                                ?? gameObject.GetComponentInChildren<T>();
                if (null == component)
                {
                    Object.Destroy(gameObject);
                }

                return component;
            }
            
            return cast;
        }
    }
}