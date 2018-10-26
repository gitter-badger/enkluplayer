using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
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

        /// <summary>
        /// Called to get reference to primary anchor
        /// </summary>
        WorldAnchorWidget Anchor { get; }

        /// <summary>
        /// Repositions an anchor's underlying GameObject relative to the primary anchor.
        /// </summary>
        /// <param name="anchor"></param>
        void PositionRelatively(WorldAnchorWidget anchor);
    }
}