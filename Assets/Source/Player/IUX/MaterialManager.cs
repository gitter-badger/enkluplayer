using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    public class MaterialManager
    {
        public enum DisplayType
        {
            Default,
            Overlay,
            Occluded,
            Hidden,
        }

        private WidgetConfig _config;

        public MaterialManager(WidgetConfig config)
        {
            _config = config;
        }

        public Material Material(Element element, DisplayType displayType)
        {
            if (element is TextPrimitive)
            {
                switch (displayType)
                {
                    case DisplayType.Occluded:
                        return _config.TextOccluded;
                    case DisplayType.Hidden:
                        return _config.TextHidden;
                    default:
                        return _config.TextOverlay;
                }
            } 
            else if (element is ActivatorPrimitive)
            {
                switch (displayType)
                {
                    case DisplayType.Occluded:
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

        public Material Material(Element element, string displayTypeStr)
        {
            if (string.IsNullOrEmpty(displayTypeStr) || !Enum.IsDefined(typeof(DisplayType), displayTypeStr))
            {
                return Material(element, DisplayType.Default);
            }
            
            var displayType = Enum.Parse(typeof(DisplayType), displayTypeStr);
            return Material(element, (DisplayType) displayType);
        }
    }
}