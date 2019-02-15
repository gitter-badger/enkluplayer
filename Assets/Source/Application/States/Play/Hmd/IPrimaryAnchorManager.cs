using System;
using System.Collections.ObjectModel;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

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
        /// Invoked whenever the anchors in a scene are identified, or when a new anchor is added.     
        /// </summary>
        event Action OnAnchorElementUpdate;
        
        /// <summary>
        /// Read only collection of currently tracked anchors.
        /// </summary>
        ReadOnlyCollection<WorldAnchorWidget> Anchors { get; }

        /// <summary>
        /// Modifies a position/rotation relative to a located anchor. The primary anchor is prioritized.
        /// The anchor used for relative positioning is returned. If all anchors aren't located, null is returned.
        /// TODO: Remove UnityEngine dependencies
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        WorldAnchorWidget RelativeTransform(ref Vector3 position, ref Quaternion rotation);
    }
}