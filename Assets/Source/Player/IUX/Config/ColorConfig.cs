using System;
using System.Collections.Generic;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
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
    
    public class ColorConfig
    {
        /// <summary>
        /// Maps a VirtualColor to a Color.
        /// </summary>
        [Serializable]
        public class VirtualColorEntry
        {
            /// <summary>
            /// Enum value.
            /// </summary>
            public VirtualColor VirtualColor;

            /// <summary>
            /// Corresponding color.
            /// </summary>
            public Color Color = Color.white;
        }

        /// <summary>
        /// Allows swapping maps for all colors.
        /// </summary>
        [Serializable]
        public class ColorProfile
        {
            /// <summary>
            /// Profile name.
            /// </summary>
            public string Name;

            /// <summary>
            /// Virtual colors for this profile.
            /// </summary>
            public List<VirtualColorEntry> VirtualColors = new List<VirtualColorEntry>();

            /// <summary>
            /// Looks up colors.
            /// </summary>
            /// <param name="virtualColor">The VirtualColor to retrieve a Color for.</param>
            /// <param name="color">The color.</param>
            /// <returns>True iff a corresponding Color was found.</returns>
            public bool TryGetColor(VirtualColor virtualColor, out Col4 color)
            {
                for (int i = 0, count = VirtualColors.Count; i < count; ++i)
                {
                    var checkVirtualColor = VirtualColors[i];
                    if (checkVirtualColor != null
                        && checkVirtualColor.VirtualColor == virtualColor)
                    {
                        color = checkVirtualColor.Color.ToCol();
                        return true;
                    }
                }

                color = Col4.White;
                return false;
            }
        }
        
        /// <summary>
        /// Name of the currently active profile.
        /// </summary>
        public string CurrentProfileName { get; private set; }
        
        /// <summary>
        /// Retrieves the current <c>ColorProfile</c>.
        /// </summary>
        public ColorProfile CurrentProfile { get; private set; }
        
        /// <summary>
        /// List of color profiles.
        /// </summary>
        public List<ColorProfile> Profiles { get; private set; }

        /// <summary>
        /// Attempts to get a color.
        /// </summary>
        /// <param name="virtualColor">The <c>VirtualColor</c> to get a Color for.</param>
        /// <param name="color">The color.</param>
        /// <returns>True iff a corresponding Color was found.</returns>
        public bool TryGetColor(VirtualColor virtualColor, out Col4 color)
        {
            var currentProfile = CurrentProfile;
            if (currentProfile == null)
            {
                color = Col4.White;
                return false;
            }

            return CurrentProfile.TryGetColor(
                virtualColor,
                out color);
        }

        /// <summary>
        /// Attempts to get a color from a string.
        /// </summary>
        /// <param name="virtualColorStr">String value.</param>
        /// <param name="color">Color output.</param>
        /// <returns>True iff the enum could be parsed and there was a corresponding
        /// Color.</returns>
        public bool TryGetColor(string virtualColorStr, out Col4 color)
        {
            var virtualColor = EnumExtensions.Parse<VirtualColor>(virtualColorStr);

            return TryGetColor(virtualColor, out color);
        }

        /// <summary>
        /// Retrieves a Color or white if one could not be found.
        /// </summary>
        /// <param name="virtualColor">The <c>VirtualColor</c> to lookup.</param>
        /// <returns></returns>
        public Col4 GetColor(VirtualColor virtualColor)
        {
            Col4 color;
            if (!TryGetColor(virtualColor, out color))
            {
                color = Col4.White;
            }

            return color;
        } 

        /// <summary>
        /// Colorize a given color.
        /// </summary>
        public Col4 Colorize(Col4 targetColor, VirtualColor virtualColor)
        {
            if (virtualColor != VirtualColor.None)
            {
                Col4 sourceColor;
                if (TryGetColor(virtualColor, out sourceColor))
                {
                    return Colorize(sourceColor, targetColor);
                }
            }

            return targetColor;
        }
        
        /// <summary>
        /// Colorizes a color.
        /// </summary>
        public static Col4 Colorize(Col4 sourceColor, Col4 targetColor)
        {
            var sourceHsbColor = HsbColor.FromColor(sourceColor);
            var targetHsbColor = HsbColor.FromColor(targetColor);
            if (Mathf.Approximately(targetHsbColor.S, 0))
            {
                sourceHsbColor.S = 0.0f;
            }

            var colorizedHsbColor = new HsbColor(sourceHsbColor.H, targetHsbColor.S, targetHsbColor.B, targetHsbColor.A);
            var colorizedColor = colorizedHsbColor.ToColor();
            return colorizedColor;
        }
    }
}