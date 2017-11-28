using System;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for assembling a <c>GameObject</c> for an <c>AssetElement</c>.
    /// </summary>
    public interface IAssetAssembler
    {
        /// <summary>
        /// Called when assembly is complete.
        /// </summary>
        event Action<GameObject> OnAssemblyComplete;

        /// <summary>
        /// Sets up the assembler.
        /// </summary>
        /// <param name="schema">The data to use to assemble the <c>AssetElement</c>.</param>
        void Setup(ElementSchema schema);
        
        /// <summary>
        /// Tears down any internal structures.
        /// </summary>
        void Teardown();
    }
}