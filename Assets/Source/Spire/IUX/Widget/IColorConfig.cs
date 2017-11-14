using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Virtual color lookup.
    /// </summary>
    public enum VirtualColor
    {
        None,

        Ready,
        Interacting,
        Disabled,

        Primary,
        Secondary,
        Tertiary,

        Positive,
        Negative,

        Highlight,
        Highlight1,
        Highlight2,
        Highlight3,
        Highlight4,
        Highlight5,

        Count
    }

    public interface IColorConfig
    {
        /// <summary>
        /// Gets a color.
        /// </summary>
        /// <param name="virtualColor">The <c>VirtualColor</c> to get a Color for.</param>
        /// <returns>True iff a corresponding Color was found.</returns>
        Col4 GetColor(VirtualColor virtualColor);
    }
}

