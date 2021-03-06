﻿#if UNITY_WSA
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// HoloLens implementation of <c>IGestureManager</c>.
    /// </summary>
    public class HoloLensGestureManager : IGestureManager
    {
        /// <summary>
        /// Internal class used to track an interaction.
        /// </summary>
        private class SourceData
        {
            /// <summary>
            /// The associated source.
            /// </summary>
            public InteractionSource Source { get; private set; }

            /// <summary>
            /// Pose data.
            /// </summary>
            public InteractionSourcePose Pose { get; private set; }

            /// <summary>
            /// True iff this interaction has been finished.
            /// </summary>
            public bool IsFinished { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public SourceData(InteractionSource source, InteractionSourcePose pose)
            {
                Source = source;
                Pose = pose;
            }

            /// <summary>
            /// Marks the interaction complate and updates pose.
            /// </summary>
            /// <param name="pose">Updated pose.</param>
            public void Complete(InteractionSourcePose pose)
            {
                IsFinished = true;

                Pose = pose;
            }

            /// <summary>
            /// Updates the interaction.
            /// </summary>
            /// <param name="pose">Updated pose.</param>
            public void Update(InteractionSourcePose pose)
            {
                Pose = pose;
            }
        }

        /// <summary>
        /// Data for each source.
        /// </summary>
        private readonly List<SourceData> _sources = new List<SourceData>();

        /// <summary>
        /// Starts coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Unique id for this call to Initialize().
        /// </summary>
        private uint _updateId;

        /// <summary>
        /// Allows generating unique ids.
        /// </summary>
        private static uint UPDATE_IDS;

        /// <inheritdoc />
        public event Action<uint> OnPointerStarted;

        /// <inheritdoc />
        public event Action<uint> OnPointerEnded;

        /// <inheritdoc />
        public event Action<uint> OnPointerPressed;

        /// <inheritdoc />
        public event Action<uint> OnPointerReleased;

        /// <inheritdoc />
        public uint[] Pointers
        {
            get
            {
                return _sources.Select(source => source.Source.id).ToArray();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public HoloLensGestureManager(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Log.Info(this, "Initializing HoloLensGestureManager.");

            InteractionManager.InteractionSourceDetected += Interactions_OnSourceDetected;
            InteractionManager.InteractionSourceLost += Interactions_OnSourceLost;
            InteractionManager.InteractionSourceUpdated += Interactions_OnSourceUpdated;
            InteractionManager.InteractionSourcePressed += Interactions_OnSourcePressed;
            InteractionManager.InteractionSourceReleased += Interactions_OnSourceReleased;
            
            _bootstrapper.BootstrapCoroutine(Update());
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Log.Info(this, "Uninitializing HoloLensGestureManager.");

            InteractionManager.InteractionSourceDetected -= Interactions_OnSourceDetected;
            InteractionManager.InteractionSourceLost -= Interactions_OnSourceLost;
            InteractionManager.InteractionSourceUpdated -= Interactions_OnSourceUpdated;
            InteractionManager.InteractionSourcePressed -= Interactions_OnSourcePressed;
            InteractionManager.InteractionSourceReleased -= Interactions_OnSourceReleased;

            _updateId = 0;
        }

        /// <inheritdoc />
        public bool TryGetPointerOrigin(uint id, out Vector3 position)
        {
            var data = Data(id);
            if (null == data)
            {
                position = Vector3.zero;
                return false;
            }

            return data.Pose.TryGetPosition(out position);
        }

        /// <inheritdoc />
        public bool TryGetPointerForward(uint id, out Vector3 forward)
        {
            var data = Data(id);
            if (null == data)
            {
                forward = Vector3.zero;
                return false;
            }

            return data.Pose.TryGetForward(out forward);
        }

        /// <inheritdoc />
        public bool TryGetPointerUp(uint id, out Vector3 up)
        {
            var data = Data(id);
            if (null == data)
            {
                up = Vector3.zero;
                return false;
            }

            return data.Pose.TryGetUp(out up);
        }

        /// <inheritdoc />
        public bool TryGetPointerRight(uint id, out Vector3 right)
        {
            var data = Data(id);
            if (null == data)
            {
                right = Vector3.zero;
                return false;
            }

            return data.Pose.TryGetRight(out right);
        }

        /// <inheritdoc />
        public bool TryGetPointerRotation(uint id, out Quaternion rotation)
        {
            var data = Data(id);
            if (null == data)
            {
                rotation = Quaternion.identity;
                return false;
            }

            return data.Pose.TryGetRotation(out rotation);
        }

        /// <inheritdoc />
        public bool TryGetPointerVelocity(uint id, out Vector3 velocity)
        {
            var data = Data(id);
            if (null == data)
            {
                velocity = Vector3.zero;
                return false;
            }

            return data.Pose.TryGetVelocity(out velocity);
        }

        /// <summary>
        /// Called every frame to trim out sources that are finished. This
        /// allows query functions to be called after a source is finished.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Update()
        {
            // We need to tie this loop with a specific call to Initialize--so
            // generate an id and check it every frame. This is more robust than
            // a bool, where calling Initialize twice without an update in between
            // would mean there would be multiple loops ticking.
            var id = _updateId = ++UPDATE_IDS;
            while (id == _updateId)
            {
                // purge
                for (var i = _sources.Count - 1; i >= 0; i--)
                {
                    if (_sources[i].IsFinished)
                    {
                        _sources.RemoveAt(i);
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Retrieves the <c>SourceData</c> for a specific source id.
        /// </summary>
        /// <param name="id">The unique id of the source.</param>
        /// <returns></returns>
        private SourceData Data(uint id)
        {
            for (int i = 0, len = _sources.Count; i < len; i++)
            {
                var source = _sources[i];
                if (source.Source.id == id)
                {
                    return source;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when a source has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Interactions_OnSourceDetected(InteractionSourceDetectedEventArgs @event)
        {
            var id = @event.state.source.id;
            var source = new SourceData(@event.state.source, @event.state.sourcePose);
            _sources.Add(source);

            if (null != OnPointerStarted)
            {
                OnPointerStarted(id);
            }
        }

        /// <summary>
        /// Called when a source has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Interactions_OnSourceLost(InteractionSourceLostEventArgs @event)
        {
            var id = @event.state.source.id;
            var data = Data(@event.state.source.id);
            if (null == data)
            {
                Log.Warning(this, "Received a gesture update event for an untracked source.");
                return;
            }

            data.Complete(@event.state.sourcePose);

            if (null != OnPointerEnded)
            {
                OnPointerEnded(id);
            }
        }

        /// <summary>
        /// Called when a source has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Interactions_OnSourceUpdated(InteractionSourceUpdatedEventArgs @event)
        {
            var data = Data(@event.state.source.id);
            if (null == data)
            {
                Log.Warning(this, "Received a gesture update event for an untracked source.");
                return;
            }

            data.Update(@event.state.sourcePose);
        }

        /// <summary>
        /// Called when a source has been pressed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Interactions_OnSourcePressed(InteractionSourcePressedEventArgs @event)
        {
            var id = @event.state.source.id;
            var data = Data(id);
            if (null == data)
            {
                Log.Warning(this, "Recieved a gesture pressed event for an untracked source.");
                return;
            }

            if (null != OnPointerPressed)
            {
                OnPointerPressed(id);
            }
        }
        
        /// <summary>
        /// Called when a source has been released.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Interactions_OnSourceReleased(InteractionSourceReleasedEventArgs @event)
        {
            var id = @event.state.source.id;
            var data = Data(id);
            if (null == data)
            {
                Log.Warning(this, "Recieved a gesture released event for an untracked source.");
                return;
            }

            if (null != OnPointerReleased)
            {
                OnPointerReleased(id);
            }
        }
    }
}
#endif