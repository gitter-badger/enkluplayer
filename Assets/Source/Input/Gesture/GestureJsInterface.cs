using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Jint.Native;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    [JsInterface("gestures")]
    public class GestureJsInterface
    {
        public class PoseJs
        {
            private readonly IGestureManager _gestures;
            private readonly uint _id;

            public PoseJs(IGestureManager gestures, uint id)
            {
                _gestures = gestures;
                _id = id;
            }

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
        }

        private const string POINTER_STARTED = "pointerstarted";
        private const string POINTER_ENDED = "pointerended";

        private readonly IGestureManager _gestures;

        private readonly Dictionary<string, List<Func<JsValue, JsValue[], JsValue>>> _listeners = new Dictionary<string, List<Func<JsValue, JsValue[], JsValue>>>
        {
            { POINTER_STARTED, new List<Func<JsValue, JsValue[], JsValue>>() },
            { POINTER_ENDED, new List<Func<JsValue, JsValue[], JsValue>>() }
        };

        public GestureJsInterface(IGestureManager gestures)
        {
            _gestures = gestures;
            _gestures.OnPointerStarted += Gestures_OnPointerStarted;
            _gestures.OnPointerEnded += Gestures_OnPointerEnded;
        }

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
        
        public void on(string eventName, Func<JsValue, JsValue[], JsValue> callback)
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

        public void off(string eventName, Func<JsValue, JsValue[], JsValue> callback)
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

        private void Gestures_OnPointerStarted(uint id)
        {
            var parameters = new[] { new JsValue(id.ToString()) };

            var list = _listeners[POINTER_STARTED].ToArray();
            for (int i = 0, len = list.Length; i < len; i++)
            {
                list[i](null, parameters);
            }
        }

        private void Gestures_OnPointerEnded(uint id)
        {
            var parameters = new[] { new JsValue(id.ToString()) };

            var list = _listeners[POINTER_ENDED].ToArray();
            for (int i = 0, len = list.Length; i < len; i++)
            {
                list[i](null, parameters);
            }
        }
    }
}