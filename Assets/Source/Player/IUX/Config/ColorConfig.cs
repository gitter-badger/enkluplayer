using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages colors.
    /// </summary>
    public class ColorConfig : MonoBehaviour, IColorConfig
    {
        [FormerlySerializedAs("CurrentProfileName")] [SerializeField] 
        private string _currentProfileName;

        [FormerlySerializedAs("Profiles")] [SerializeField]
        private List<ColorProfile> _profiles = new List<ColorProfile>();
        
        /// <inheritdoc />
        public string CurrentProfileName
        {
            get { return _currentProfileName; }
        }

        /// <inheritdoc />
        public ColorProfile CurrentProfile
        {
            get
            {
                if (Profiles != null
                    && Profiles[0] != null)
                {
                    for (int i = 0, count = Profiles.Count; i < count; ++i)
                    {
                        var profile = Profiles[i];
                        if (profile != null
                            && profile.Name == CurrentProfileName)
                        {
                            return profile;
                        }
                    }

                    return Profiles[0];
                }

                return null;
            }
        }

        /// <inheritdoc />
        public List<ColorProfile> Profiles
        {
            get { return _profiles; }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool TryGetColor(string virtualColorStr, out Col4 color)
        {
            var virtualColor = EnumExtensions.Parse<VirtualColor>(virtualColorStr);

            return TryGetColor(virtualColor, out color);
        }

        /// <inheritdoc />
        public Col4 GetColor(VirtualColor virtualColor)
        {
            Col4 color;
            if (!TryGetColor(virtualColor, out color))
            {
                color = Col4.White;
            }

            return color;
        }

        /// <inheritdoc />
        public Col4 Colorize(
            Col4 targetColor,
            VirtualColor virtualColor)
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