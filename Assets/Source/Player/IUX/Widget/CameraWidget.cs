using System.Collections.Generic;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Marks a location for a camera.
    /// </summary>
    public class CameraWidget : Widget
    {
        /// <summary>
        /// Configuration for a camera.
        /// </summary>
        private struct CameraConfiguration
        {
            /// <summary>
            /// Camera configuration variables.
            /// </summary>
            public int Fov;
            public float NearPlane;
            public float FarPlane;
            public Col4 ClearColor;

            /// <summary>
            /// Applies self to camera.
            /// </summary>
            /// <param name="camera">The camera to apply to.</param>
            public void Apply(Camera camera)
            {
                if (null == camera)
                {
                    return;
                }

                camera.fieldOfView = Fov;
                camera.nearClipPlane = NearPlane;
                camera.farClipPlane = FarPlane;
                camera.backgroundColor = ClearColor.ToColor();
            }
        }

        private static readonly Dictionary<Camera, CameraWidget> _CameraMap = new Dictionary<Camera, CameraWidget>();

        private ElementSchemaProp<int> _fovProp;
        private ElementSchemaProp<float> _nearProp;
        private ElementSchemaProp<float> _farProp;
        private ElementSchemaProp<Col4> _clearProp;

        private CameraConfiguration _config;
        private Camera _camera;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CameraWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors)
            : base(gameObject, layers, tweens, colors)
        {
            //
        }

        public void Apply(Camera camera)
        {
            CameraWidget current;
            if (_CameraMap.TryGetValue(camera, out current))
            {
                current._camera = null;
            }

            _CameraMap[camera] = this;
            _config.Apply(camera);
        }

        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            _fovProp = Schema.Get<int>("camera.fov");
            _nearProp = Schema.Get<float>("camera.near");
            _farProp = Schema.Get<float>("camera.far");
            _clearProp = Schema.Get<Col4>("camera.clear");

            _fovProp.OnChanged += Prop_OnChanged;
            _nearProp.OnChanged += Prop_OnChanged;
            _farProp.OnChanged += Prop_OnChanged;
            _clearProp.OnChanged += Prop_OnChanged;
        }

        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _fovProp.OnChanged -= Prop_OnChanged;
            _nearProp.OnChanged -= Prop_OnChanged;
            _farProp.OnChanged -= Prop_OnChanged;
            _clearProp.OnChanged -= Prop_OnChanged;
        }

        private void UpdateConfiguration()
        {
            _config = new CameraConfiguration
            {
                Fov = _fovProp.Value,
                NearPlane = _nearProp.Value,
                FarPlane = _farProp.Value,
                ClearColor = _clearProp.Value
            };

            _config.Apply(_camera);
        }

        private void Prop_OnChanged(ElementSchemaProp<int> prop, int prev, int next)
        {
            UpdateConfiguration();
        }

        private void Prop_OnChanged(ElementSchemaProp<float> prop, float prev, float next)
        {
            UpdateConfiguration();
        }

        private void Prop_OnChanged(ElementSchemaProp<Col4> prop, Col4 prev, Col4 next)
        {
            UpdateConfiguration();
        }
    }
}