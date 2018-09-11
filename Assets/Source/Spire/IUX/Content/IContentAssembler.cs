using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for assembling a <c>GameObject</c> for <c>ContentWidget</c>.
    /// </summary>
    public interface IContentAssembler
    {
        /// <summary>
        /// Retrieves the bounds.
        /// </summary>
        Bounds Bounds { get; }

        /// <summary>
        /// Retrieves the current assembled GameObject. May be null during loading.
        /// </summary>
        GameObject Assembly { get; }

        /// <summary>
        /// Called when assembly is complete.
        /// </summary>
        event Action<GameObject> OnAssemblyComplete;

        /// <summary>
        /// Sets up the assembler.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="assetId">The id of the asset to setup..</param>
        void Setup(Transform transform, string assetId);

        /// <summary>
        /// Tears down any internal structures.
        /// </summary>
        void Teardown();
    }
}