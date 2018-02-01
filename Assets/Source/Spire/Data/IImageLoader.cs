using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
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
        /// Replaces templates in URLs.
        /// 
        /// TODO: Refactor into URLBuilder.
        /// </summary>
        /// <param name="template">String template.</param>
        /// <param name="replacement">Value to replace with.</param>
        /// <returns></returns>
        IImageLoader Replace(string template, string replacement);

        /// <summary>
        /// Replaces a protocol in URLs.
        /// 
        /// TODO: Refactor into URLBuilder.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        IImageLoader ReplaceProtocol(string protocol, string replacement);

        /// <summary>
        /// Loads an image into a texture.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        IAsyncToken<ManagedTexture> Load(string url);
    }
}