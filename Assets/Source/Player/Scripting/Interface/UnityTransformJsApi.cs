using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Underlying Dummy <see cref="IElementTransformJsApi"/> implementation. Values are populated from Unity's transform.
    /// </summary>
    public class UnityTransformJsApi : IElementTransformJsApi
    {
        /// <summary>
        /// Cached Unity Transform, used when setting values from the JsApi.
        /// </summary>
        public readonly Transform UnityTransform;

        /// <summary>
        /// Creates a new UnityTransformJsApi
        /// </summary>
        /// <param name="unityTransform"></param>
        public UnityTransformJsApi(Transform unityTransform)
        {
            UnityTransform = unityTransform;
        }

        /// <summary>
        /// Backing position value.
        /// </summary>
        private Vec3 _position;

        /// <summary>
        /// Backing rotation value.
        /// </summary>
        private Quat _rotation;

        /// <summary>
        /// Backing scale value.
        /// </summary>
        private Vec3 _scale;

        /// <inheritdoc />
        public Vec3 position
        {
            get
            {
                return _position;
            }
            set
            {
                Log.Warning(this, "Attempting to set UnityTransform value");
                //UnityTransform.position = new Vector3(_position.x, _position.y, _position.z);
            }
        }

        /// <inheritdoc />
        public Quat rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                Log.Warning(this, "Attempting to set UnityTransform value");
                //UnityTransform.rotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
            }
        }

        /// <inheritdoc />
        public Vec3 scale
        {
            get
            {
                return _scale;
            }
            set
            {
                Log.Warning(this, "Attempting to set UnityTransform value");
                //UnityTransform.localScale = new Vector3(_scale.x, _scale.y, _scale.z);
            }
        }

        /// <inheritdoc />
        public Vec3 worldPosition
        {
            get { return UnityTransform.position.ToVec(); }
        }

        /// <inheritdoc />
        public Quat worldRotation
        {
            get { return UnityTransform.rotation.ToQuat();  }
        }

        /// <inheritdoc />
        public Vec3 worldScale
        {
            get { return UnityTransform.lossyScale.ToVec(); }
        }

        /// <summary>
        /// Updates <see cref="position"/>, <see cref="rotation"/>, and <see cref="scale"/> to match the underlying Unity Transform's values.
        /// </summary>
        [DenyJsAccess]
        public void UpdateJsTransform()
        {
            _position.Set(
                UnityTransform.localPosition.x,
                UnityTransform.localPosition.y,
                UnityTransform.localPosition.z);
            _rotation.Set(
                UnityTransform.localRotation.x,
                UnityTransform.localRotation.y,
                UnityTransform.localRotation.z,
                UnityTransform.localRotation.w);
            _scale.Set(
                UnityTransform.localScale.x,
                UnityTransform.localScale.y,
                UnityTransform.localScale.z);
        }

        /// <inheritdoc />
        public Vec3 positionRelativeTo(IEntityJs entity)
        {
            return worldPosition - entity.transform.worldPosition;
        }
    }
}