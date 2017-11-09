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
        /// Called when assembly is complete.
        /// </summary>
        event Action<GameObject> OnAssemblyComplete;
        
        /// <summary>
        /// Sets up the assembler.
        /// </summary>
        /// <param name="data">The data to use to assemble the <c>Content</c>.</param>
        void Setup(ContentData data);

        /// <summary>
        /// Tears down any internal structures.
        /// </summary>
        void Teardown();
    }
}