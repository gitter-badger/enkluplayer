using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for assembling an asset.
    /// </summary>
    public interface IAssetAssembler
    {
        /// <summary>
        /// Retrieves the bounds.
        /// </summary>
        Bounds Bounds { get; }

        /// <summary>
        /// The assembled GameObject.
        /// </summary>
        GameObject Assembly { get; }

        /// <summary>
        /// Called when the assembly has been updated.
        /// </summary>
        event Action OnAssemblyUpdated;

        /// <summary>
        /// Sets up the assembler.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="assetId">The id of the asset to setup..</param>
        /// <param name="version"></param>
        void Setup(Transform transform, string assetId, int version);

        /// <summary>
        /// Tears down any internal structures.
        /// </summary>
        void Teardown();
    }
}