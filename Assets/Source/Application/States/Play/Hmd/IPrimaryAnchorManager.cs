﻿using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

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
        /// Sets up the manager. Should be called after entering edit mode.
        /// </summary>
        void Setup();

        /// <summary>
        /// Tears down the manager. Should be called before leaving edit mode.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Calculates the relative offsets from the primary anchor.
        /// </summary>
        void CalculateOffsets(
            Vec3 position,
            Vec3 eulerAngles, 
            Action<Vec3, Vec3> callback);
    }
}