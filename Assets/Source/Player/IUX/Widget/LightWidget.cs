using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Widget for a light source.
    /// </summary>
    public class LightWidget : Widget
    {
        /// <summary>
        /// Unity Light.
        /// </summary>
        private Light _light;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _typeProp;
        private ElementSchemaProp<float> _intensityProp;
        private ElementSchemaProp<string> _shadowsProp;
        private ElementSchemaProp<Col4> _colorProp;
        private ElementSchemaProp<float> _pointRangeProp;
        private ElementSchemaProp<float> _spotAngleProp;
        private ElementSchemaProp<float> _spotRangeProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LightWidget(
            GameObject gameObject,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors)
            : base(gameObject, layers, tweens, colors)
        {
            //
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren() 
        {
            base.LoadInternalBeforeChildren();
            
            _light = GameObject.AddComponent<Light>();
            
            // collider
            var collider = EditCollider;
            if (null != collider)
            {
                collider.center = Vector3.zero;
                collider.size = 0.25f * Vector3.one;
            }

            // determines light type
            _typeProp = Schema.GetOwn("lightType", LightType.Directional.ToString());
            
            // common
            _intensityProp = Schema.GetOwn("intensity", 1f);
            _shadowsProp = Schema.GetOwn("shadows", LightShadows.None.ToString());
            _colorProp = Schema.GetOwn("color", Col4.White);

            // point
            _pointRangeProp = Schema.GetOwn("point.range", 1f);

            // spot
            _spotRangeProp = Schema.GetOwn("spot.range", 1f);
            _spotAngleProp = Schema.GetOwn("spot.angle", 30f);
            
            // listen to props
            _typeProp.OnChanged += Type_OnChanged;
            _intensityProp.OnChanged += Intensity_OnChanged;
            _shadowsProp.OnChanged += Shadows_OnChanged;
            _colorProp.OnChanged += Color_OnChanged;
            _pointRangeProp.OnChanged += PointRange_OnChanged;
            _spotRangeProp.OnChanged += SpotRange_OnChanged;
            _spotAngleProp.OnChanged += SpotAngle_OnChanged;

            UpdateLight();
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();
            
            _typeProp.OnChanged -= Type_OnChanged;
            _intensityProp.OnChanged -= Intensity_OnChanged;
            _shadowsProp.OnChanged -= Shadows_OnChanged;
            _colorProp.OnChanged -= Color_OnChanged;
            _pointRangeProp.OnChanged -= PointRange_OnChanged;
            _spotAngleProp.OnChanged -= SpotAngle_OnChanged;
            
            Object.Destroy(_light);
            _light = null;
        }
        
        /// <summary>
        /// Updates all properties of light.
        /// </summary>
        private void UpdateLight()
        {
            _light.type = ToLightType(_typeProp.Value);
            _light.intensity = _intensityProp.Value;
            _light.shadows = ToLightShadows(_shadowsProp.Value);
            _light.color = _colorProp.Value.ToColor();
            
            if (_light.type == LightType.Point)
            {
                _light.range = _pointRangeProp.Value;
            }
            else if (_light.type == LightType.Spot)
            {
                _light.range = _spotRangeProp.Value;
            }

            _light.spotAngle = _spotAngleProp.Value;
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void SpotAngle_OnChanged(ElementSchemaProp<float> prop, float prev, float next)
        {
            _light.spotAngle = next;
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void SpotRange_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            _light.range = next;
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void PointRange_OnChanged(ElementSchemaProp<float> prop, float prev, float next)
        {
            _light.range = next;
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void Color_OnChanged(ElementSchemaProp<Col4> prop, Col4 prev, Col4 next)
        {
            _light.color = next.ToColor();
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void Shadows_OnChanged(ElementSchemaProp<string> prop, string prev, string next)
        {
            _light.shadows = ToLightShadows(next);
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void Intensity_OnChanged(ElementSchemaProp<float> prop, float prev, float next)
        {
            _light.intensity = next;
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        private void Type_OnChanged(ElementSchemaProp<string> prop, string prev, string next)
        {
            UpdateLight();
        }
        
        /// <summary>
        /// Parses light type.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <returns></returns>
        private static LightType ToLightType(string value)
        {
            try
            {
                return (LightType) Enum.Parse(typeof(LightType), value);
            }
            catch
            {
                return LightType.Directional;
            }
        }
        
        /// <summary>
        /// Parses light shadows.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <returns></returns>
        private static LightShadows ToLightShadows(string value)
        {
            try
            {
                return (LightShadows) Enum.Parse(typeof(LightShadows), value);
            }
            catch
            {
                return LightShadows.None;
            }
        }
    }
}