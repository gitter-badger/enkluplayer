using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Interface for loading images.
    /// </summary>
    public interface IImageLoader
    {
        /// <summary>
        /// Replaces templates in URLs.
        /// </summary>
        /// <param name="template">String template.</param>
        /// <param name="replacement">Value to replace with.</param>
        /// <returns></returns>
        IImageLoader Replace(string template, string replacement);

        /// <summary>
        /// Loads an image into a texture.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        IAsyncToken<Func<Texture2D, bool>> Load(string url);
    }
}