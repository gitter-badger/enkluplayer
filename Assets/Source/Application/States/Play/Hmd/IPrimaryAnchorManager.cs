using System;
using CreateAR.SpirePlayer.IUX;

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
        /// Sets up the manager.
        /// </summary>
        void Setup();

        /// <summary>
        /// Tears down the manager. Should be called before leaving edit mode.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Called when the primary anchor is ready and located.
        /// </summary>
        /// <param name="ready">The callback to call when ready.</param>
        void OnPrimaryLocated(Action ready);
    }
}