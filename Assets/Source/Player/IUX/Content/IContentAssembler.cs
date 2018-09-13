using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        /// Called when assembly is complete.
        /// </summary>
        IMutableAsyncToken<GameObject> OnAssemblyComplete { get; }

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