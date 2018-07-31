using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that tracks the primary world anchor.
    /// </summary>
    public interface IPrimaryAnchorManager
    {
        /// <summary>
        /// Primary world anchor status.
        /// </summary>
        WorldAnchorWidget.WorldAnchorStatus Status { get; }
        
        /// <summary>
        /// Sets up the manager. Should be called after entering edit mode.
        /// </summary>
        void Setup();

        /// <summary>
        /// Tears down the manager. Should be called before leaving edit mode.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Calculates the relative offsets of an anchor from the primary anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="positionOffset">The positional offset.</param>
        /// <param name="eulerOffset">The euler offset.</param>
        /// <returns></returns>
        bool CalculateOffsets(
            WorldAnchorWidget anchor, 
            out Vector3 positionOffset,
            out Vector3 eulerOffset);
    }
}