using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Allows for listening of gestures, and querying state related to active ones.
    /// </summary>
    public interface IGestureManager
    {
        /// <summary>
        /// Dispatched when a new gesture starts. Multiple gestures can be tracked concurrently.
        /// </summary>
        event Action<uint> OnPointerStarted;

        /// <summary>
        /// Dispatched when an existing gesture ends.
        /// </summary>
        event Action<uint> OnPointerEnded;

        /// <summary>
        /// Dispatched when an existing gesture is pressed.
        /// </summary>
        event Action<uint> OnPointerPressed;

        /// <summary>
        /// Dispatched when an existing gesture is released. 
        /// </summary>
        event Action<uint> OnPointerReleased;

        /// <summary>
        /// Array of active gesture IDs.
        /// </summary>
        uint[] Pointers { get; }

        /// <summary>
        /// Initialize the gesture tracking.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Uninitialize the gesture tracking.
        /// </summary>
        void Uninitialize();

        /// <summary>
        /// Attempt to get the gesture's world forward.
        /// </summary>
        /// <param name="id">Gesture to query</param>
        /// <param name="position">World position.</param>
        /// <returns></returns>
        bool TryGetPointerOrigin(uint id, out Vector3 position);

        /// <summary>
        /// Attempt to get the gesture's forward direction.
        /// </summary>
        /// <param name="id">Gesture to query.</param>
        /// <param name="forward">World relative.</param>
        /// <returns></returns>
        bool TryGetPointerForward(uint id, out Vector3 forward);

        /// <summary>
        /// Attempt to get the gesture's up direction.
        /// </summary>
        /// <param name="id">Gesture to query.</param>
        /// <param name="up">World relative.</param>
        /// <returns></returns>
        bool TryGetPointerUp(uint id, out Vector3 up);

        /// <summary>
        /// Attempt to get the gesture's right direction.
        /// </summary>
        /// <param name="id">Gesture to query.</param>
        /// <param name="right">World relative.</param>
        /// <returns></returns>
        bool TryGetPointerRight(uint id, out Vector3 right);

        /// <summary>
        /// Attempt to get the gesture's rotation.
        /// </summary>
        /// <param name="id">Gesture to query.</param>
        /// <param name="rotation">World relative.</param>
        /// <returns></returns>
        bool TryGetPointerRotation(uint id, out Quaternion rotation);

        /// <summary>
        /// Attempt to get the gesture's linear velocity.
        /// </summary>
        /// <param name="id">Gesture to Query.</param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        bool TryGetPointerVelocity(uint id, out Vector3 velocity);
    }
}
