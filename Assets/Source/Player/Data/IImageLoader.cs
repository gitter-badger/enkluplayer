using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Temporary.
    /// </summary>
    public class ManagedTexture
    {
        public readonly Texture2D Source;

        public ManagedTexture(Texture2D source)
        {
            Source = source;
        }

        public void Release()
        {

        }
    }

    /// <summary>
    /// Interface for loading images.
    /// </summary>
    public interface IImageLoader
    {
        /// <summary>
        /// For building URLs.
        /// </summary>
        UrlFormatterCollection Urls { get; }

        /// <summary>
        /// Loads an image into a texture.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        IAsyncToken<ManagedTexture> Load(string url);
    }
}