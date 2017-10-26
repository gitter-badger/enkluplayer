﻿namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for objects that need to be layered.
    /// </summary>
    public interface ILayerable
    {
        /// <summary>
        /// True iff the object is visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Describes how the layering affects the object.
        /// </summary>
        LayerMode LayerMode { get; }
    }
}