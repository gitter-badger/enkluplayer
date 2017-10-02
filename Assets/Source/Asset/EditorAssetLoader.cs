using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer;
using Object = UnityEngine.Object;

namespace CreateAR.Spire
{
    /// <summary>
    /// Unity Editor implementation.
    /// </summary>
    public class EditorAssetLoader : IAssetLoader
    {
        /// <inheritdoc cref="IAssetLoader"/>
        public IAsyncToken<Object> Load(AssetInfo info, out LoadProgress progress)
        {
            progress = new LoadProgress();

#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(info.Guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (null == asset)
            {
                return new AsyncToken<Object>(new Exception(string.Format(
                    "Could not find asset {0}",
                    info)));
            }

            return new AsyncToken<Object>(asset);
#else
            return new AsyncToken<Object>(new System.NotImplementedException());
#endif
        }
    }
}
