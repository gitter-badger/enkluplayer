using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    [JsInterface("gestures")]
    public class GestureJsInterface
    {
        
    }

    public interface IGestureManager
    {
        event Action<uint> OnPointerStarted;
        event Action<uint> OnPointerEnded;

        uint[] Pointers { get; }

        void Initialize();
        void Uninitialize();

        bool TryGetPointerOrigin(uint id, out Vector3 position);
        bool TryGetPointerForward(uint id, out Vector3 position);
        bool TryGetPointerUp(uint id, out Vector3 position);
        bool TryGetPointerRight(uint id, out Vector3 position);
        bool TryGetPointerRotation(uint id, out Quaternion rotation);
    }
}
