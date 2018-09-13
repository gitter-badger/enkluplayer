using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an interface thaqt can import a mesh at runtime.
    /// </summary>
    public interface IScanImporter
    {
        /// <summary>
        /// Stops the coroutine.
        /// </summary>
        void Stop();

        /// <summary>
        /// Imports an obj from source bytes. The callback is passed a function
        /// that will construct the meshes on a GameObject. This allows the 
        /// caller of this method to decide when to actually create the meshes.
        /// </summary>
        /// <param name="bytes">The obj source text.</param>
        /// <param name="callback">The callback.</param>
        void Import(
            byte[] bytes,
            Action<Exception, Func<GameObject, Bounds>> callback);
    }
}