using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for assembling a <c>GameObject</c> for <c>Content</c>.
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
        event Action<GameObject> OnAssemblyComplete;
        
        /// <summary>
        /// Sets up the assembler.
        /// </summary>
        /// <param name="data">The data to use to assemble the <c>Content</c>.</param>
        void Setup(ContentData data);

        /// <summary>
        /// Updates the material.
        /// </summary>
        /// <param name="material">Updated material data.</param>
        void UpdateMaterialData(MaterialData material);

        /// <summary>
        /// Tears down any internal structures.
        /// </summary>
        void Teardown();
    }
}