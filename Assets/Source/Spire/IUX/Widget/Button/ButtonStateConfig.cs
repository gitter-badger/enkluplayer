using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Holds values for a specific button state.
    /// </summary>
    [Serializable]
    public class ButtonStateConfig
    {
        /// <summary>
        /// Main color.
        /// </summary>
        public VirtualColor Color = VirtualColor.Ready;

        /// <summary>
        /// Color of text.
        /// </summary>
        public VirtualColor CaptionColor = VirtualColor.Primary;

        /// <summary>
        /// Target scale to tween to.
        /// </summary>
        public Vector3 Scale = Vector3.one;

        /// <summary>
        /// The type of tween to use.
        /// </summary>
        public TweenType Tween;
    }
}