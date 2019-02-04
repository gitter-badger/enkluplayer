using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
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
        /// Name of the currently active profile.
        /// </summary>
        string CurrentProfileName { get; }
        
        /// <summary>
        /// Retrieves the current <c>ColorProfile</c>.
        /// </summary>
        ColorProfile CurrentProfile { get; }
        
        /// <summary>
        /// List of color profiles.
        /// </summary>
        List<ColorProfile> Profiles { get; }

        /// <summary>
        /// Attempts to get a color.
        /// </summary>
        /// <param name="virtualColor">The <c>VirtualColor</c> to get a Color for.</param>
        /// <param name="color">The color.</param>
        /// <returns>True iff a corresponding Color was found.</returns>
        bool TryGetColor(VirtualColor virtualColor, out Col4 color);
        
        /// <summary>
        /// Attempts to get a color from a string.
        /// </summary>
        /// <param name="virtualColorStr">String value.</param>
        /// <param name="color">Color output.</param>
        /// <returns>True iff the enum could be parsed and there was a corresponding
        /// Color.</returns>
        bool TryGetColor(string virtualColorStr, out Col4 color);
        
        /// <summary>
        /// Retrieves a Color or white if one could not be found.
        /// </summary>
        /// <param name="virtualColor">The <c>VirtualColor</c> to lookup.</param>
        /// <returns></returns>
        Col4 GetColor(VirtualColor virtualColor);
        
        /// <summary>
        /// Colorize a given color.
        /// </summary>
        Col4 Colorize(Col4 targetColor, VirtualColor virtualColor);
        
    }
}