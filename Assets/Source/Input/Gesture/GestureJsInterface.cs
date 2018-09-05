using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Jint.Native;
using UnityEngine;

using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// JS interface for gestures.
    /// TODO: call handlers with same context as was passed in.
    /// </summary>
    [JsInterface("gestures")]
    public class GestureJsInterface
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

        /// <summary>
        /// Gesture interface.
        /// </summary>
        private readonly IGestureManager _gestures;

        /// <summary>
        /// Tracks listeners.
        /// </summary>
        private readonly Dictionary<string, List<JsFunc>> _listeners = new Dictionary<string, List<JsFunc>>
        {
            { EVENT_POINTER_STARTED, new List<JsFunc>() },
            { EVENT_POINTER_ENDED, new List<JsFunc>() }
        };

        /// <summary>
        /// Constructor.
        /// </summary>
        public GestureJsInterface(IGestureManager gestures)
        {
            _gestures = gestures;
            _gestures.OnPointerStarted += Gestures_OnPointerStarted;
            _gestures.OnPointerEnded += Gestures_OnPointerEnded;
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
        /// Adds a listener.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="callback">The callback.</param>
        public void on(string eventName, JsFunc callback)
        {
            List<Func<JsValue, JsValue[], JsValue>> list;
            if (_listeners.TryGetValue(eventName, out list))
            {
                list.Add(callback);
            }
            else
            {
                Log.Warning(this, "Unknown event type '{0}'.", eventName);
            }
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="callback">The callback.</param>
        public void off(string eventName, JsFunc callback)
        {
            List<Func<JsValue, JsValue[], JsValue>> list;
            if (_listeners.TryGetValue(eventName, out list))
            {
                list.Remove(callback);
            }
            else
            {
                Log.Warning(this, "Unknown event type '{0}'.", eventName);
            }
        }

        /// <summary>
        /// Called when the gestures interface fires a new pointer event.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerStarted(uint id)
        {
            var parameters = new[] { new JsValue(id.ToString()) };

            var list = _listeners[EVENT_POINTER_STARTED].ToArray();
            for (int i = 0, len = list.Length; i < len; i++)
            {
                list[i](null, parameters);
            }
        }

        /// <summary>
        /// Called when the gestures interface fires a pointer is removed.
        /// </summary>
        /// <param name="id">Unique id of the pose.</param>
        private void Gestures_OnPointerEnded(uint id)
        {
            var parameters = new[] { new JsValue(id.ToString()) };

            var list = _listeners[EVENT_POINTER_ENDED].ToArray();
            for (int i = 0, len = list.Length; i < len; i++)
            {
                list[i](null, parameters);
            }
        }
    }
}