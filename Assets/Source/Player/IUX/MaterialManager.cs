using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages getting shared Unity Material instances for primitives based on their display style.
    /// </summary>
    public class MaterialManager
    {
        /// <summary>
        /// The style in which a primitive is displayed.
        /// </summary>
        public enum DisplayStyle
        {
            /// <summary>
            /// Default. TODO: Make this dependent on Play/Edit mode.
            /// </summary>
            Default,
            
            /// <summary>
            /// Overlaid on top of everything.
            /// </summary>
            Overlay,
            
            /// <summary>
            /// Occluded in the scene.
            /// </summary>
            Occluded,
            
            /// <summary>
            /// Requires a nearby active gesture to be visible.
            /// TODO: Actually implement this.
            /// </summary>
            Hidden,
        }

        /// <summary>
        /// Configuration for Material refs.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MaterialManager(WidgetConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Gets a shared Material instance for a primitive based on a DisplayStyle.
        /// </summary>
        /// <param name="primitive">The primitive.</param>
        /// <param name="displayStyle">The display style desired.</param>
        /// <returns>A Unity material instance.</returns>
        /// <exception cref="ArgumentException">Throws if primitive isn't supported.</exception>
        public Material Material(Element primitive, DisplayStyle displayStyle)
        {
            if (primitive is TextPrimitive)
            {
                switch (displayStyle)
                {
                    case DisplayStyle.Occluded:
                        return _config.TextOccluded;
                    case DisplayStyle.Hidden:
                        return _config.TextHidden;
                    default:
                        return _config.TextOverlay;
                }
            } 
            else if (primitive is ActivatorPrimitive)
            {
                switch (displayStyle)
                {
                    case DisplayStyle.Occluded:
                        return _config.ButtonOccluded;
                    default:
                        return _config.ButtonOverlay;
                }
            }
            else
            {
                throw new ArgumentException("Element must be TextPrimitive or ActivatorPrimitive");
            }
        }

        /// <summary>
        /// Gets a shared Material instance for a primitive based on a string DisplayStyle.
        /// Parsing errors default to DisplayStyle.Default.
        /// </summary>
        /// <param name="primitive">The primitive.</param>
        /// <param name="displayTypeStr">The display style desired.</param>
        /// <returns>A Unity material instance.</returns>
        public Material Material(Element primitive, string displayTypeStr)
        {
            if (string.IsNullOrEmpty(displayTypeStr) || !Enum.IsDefined(typeof(DisplayStyle), displayTypeStr))
            {
                return Material(primitive, DisplayStyle.Default);
            }
            
            var displayType = Enum.Parse(typeof(DisplayStyle), displayTypeStr);
            return Material(primitive, (DisplayStyle) displayType);
        }
    }
}