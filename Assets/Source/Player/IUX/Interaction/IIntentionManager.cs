﻿using Enklu.Data;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Interface for intention. This is for the purposes of abstracting away
    /// Unity interfaces.
    /// </summary>
    public interface IIntentionManager
    {
        /// <summary>
        /// Current focus.
        /// </summary>
        IInteractable Focus { get; }

        /// <summary>
        /// Origin of the intention (i.e. the user's eyes).
        /// </summary>
        Vec3 Origin { get; }

        /// <summary>
        /// Forward direction.
        /// </summary>
        Vec3 Forward { get; }

        /// <summary>
        /// Up direction.
        /// </summary>
        Vec3 Up { get; }

        /// <summary>
        /// Right direction.
        /// </summary>
        Vec3 Right { get; }

        /// <summary>
        /// Measure of stability.
        /// </summary>
        float Stability { get; }

        /// <summary>
        /// True if visible to user.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="fovScale"></param>
        /// <returns></returns>
        bool IsVisible(Vec3 position, float fovScale = 1.0f);
    }
}