using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Enklu.Data;
using Jint.Native;
using UnityEngine;

using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JS interface for gestures.
    /// TODO: call handlers with same context as was passed in.
    /// </summary>
    [JsInterface("gestures")]
    public class GestureJsInterface : JsEventDispatcher
    {
        /// <summary>
        /// Pose data.
        /// </summary>
        public class PoseJs
        {
            /// <summary>
            /// C# gesture interface.
            /// </summary>
            private readonly IGestureManager _gestures;

            /// <summary>
            /// Unique id of gesture source.
            /// </summary>
            private readonly uint _id;

            /// <summary>
            /// Constructor.
            /// </summary>
            public PoseJs(IGestureManager gestures, uint id)
            {
                _gestures = gestures;
                _id = id;
            }

            /// <summary>
            /// Retrieves the origin of a pose.
            /// </summary>
            public Vec3 origin
            {
                get
                {
                    Vector3 vec;
                    if (_gestures.TryGetPointerOrigin(_id, out vec))
                    {
                        return vec.ToVec();
                    }

                    return Vec3.Zero;
                }
            }

            /// <summary>
            /// Retrieves the up of a pose.
            /// </summary>
            public Vec3 up
            {
                get
                {
                    Vector3 vec;
                    if (_gestures.TryGetPointerUp(_id, out vec))
                    {
                        return vec.ToVec();
                    }

                    return Vec3.Zero;
                }
            }

            /// <summary>
            /// Retrieves the right of a pose.
            /// </summary>
            public Vec3 right
            {
                get
                {
                    Vector3 vec;
                    if (_gestures.TryGetPointerRight(_id, out vec))
                    {
                        return vec.ToVec();
                    }

                    return Vec3.Zero;
                }
            }

            /// <summary>
            /// Retrieves the forward of a pose.
            /// </summary>
            public Vec3 forward
            {
                get
                {
                    Vector3 vec;
                    if (_gestures.TryGetPointerForward(_id, out vec))
                    {
                        return vec.ToVec();
                    }

                    return Vec3.Zero;
                }
            }

            /// <summary>
            /// Retrieves the rotation of a pose.
            /// </summary>
            public Quat rotation
            {
                get
                {
                    Quaternion quat;
                    if (_gestures.TryGetPointerRotation(_id, out quat))
                    {
                        return quat.ToQuat();
                    }

                    return Quat.Identity;
                }
            }

            /// <summary>
            /// Retrieves the velocity of a pose.
            /// </summary>
            public Vec3 velocity
            {
                get
                {
                    Vector3 velocity;
                    if (_gestures.TryGetPointerVelocity(_id, out velocity))
                    {
                        return velocity.ToVec();
                    }

                    return Vec3.Zero;
                }
            }
        }

        /// <summary>
        /// Event names.
        /// </summary>
        private const string EVENT_POINTER_STARTED = "pointerstarted";
        private const string EVENT_POINTER_ENDED = "pointerended";
        private const string EVENT_POINTER_PRESSED = "pointerpressed";
        private const string EVENT_POINTER_RELEASED = "pointerreleased";

        /// <summary>
        /// Gesture interface.
        /// </summary>
        private readonly IGestureManager _gestures;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GestureJsInterface(IGestureManager gestures)
        {
            _gestures = gestures;
            _gestures.OnPointerStarted += Gestures_OnPointerStarted;
            _gestures.OnPointerEnded += Gestures_OnPointerEnded;
            _gestures.OnPointerPressed += Gestures_OnPointerPressed;
            _gestures.OnPointerReleased += Gestures_OnPointerReleased;
        }

        /// <summary>
        /// Retrieves pose data for a specific id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public PoseJs pose(string id)
        {
            uint uintId;
            if (!uint.TryParse(id, out uintId))
            {
                return null;
            }

            var pointers = _gestures.Pointers;
            for (int i = 0, len = pointers.Length; i < len; i++)
            {
                if (pointers[i] == uintId)
                {
                    return new PoseJs(_gestures, uintId);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Called when the gestures interface fires a new pointer event.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerStarted(uint id)
        {
            try
            {
                dispatch(EVENT_POINTER_STARTED, id.ToString());
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch pointer started event : {0}.", exception);
            }
        }

        /// <summary>
        /// Called when the gestures interface fires a pointer is removed.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerEnded(uint id)
        {
            try
            {
                dispatch(EVENT_POINTER_ENDED, id.ToString());
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch pointer ended event : {0}.", exception);
            }
        }

        /// <summary>
        /// Called when the gestures interface fires a pointer is pressed.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerPressed(uint id)
        {
            try
            {
                dispatch(EVENT_POINTER_PRESSED, id.ToString());
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch pointer pressed event : {0}.", exception);
            }
        }

        /// <summary>
        /// Called when the gestures interface fires a pointer is released.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerReleased(uint id)
        {
            try
            {
                dispatch(EVENT_POINTER_RELEASED, id.ToString());
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch pointer released event : {0}.", exception);
            }
        }
    }
}