﻿using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Passthrough implementation of gestures.
    /// </summary>
    public class PassthroughGestureManager : IGestureManager
    {
        /// <inheritdoc />
        public event Action<uint> OnPointerStarted;

        /// <inheritdoc />
        public event Action<uint> OnPointerEnded;

        /// <inheritdoc />
        public uint[] Pointers
        {
            get
            {
                return new uint[0];
            }
        }

        /// <inheritdoc />
        public void Initialize()
        {
            
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            
        }

        /// <inheritdoc />
        public bool TryGetPointerOrigin(uint id, out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetPointerForward(uint id, out Vector3 forward)
        {
            forward = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetPointerUp(uint id, out Vector3 up)
        {
            up = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetPointerRight(uint id, out Vector3 right)
        {
            right = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetPointerRotation(uint id, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetPointerVelocity(uint id, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            return false;
        }
    }
}