using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Represents the user, for JsApi libraries to tie into. Currently not exported for scripts to use.
    /// </summary>
    public class PlayerJs : InjectableMonoBehaviour, IEntityJs
    {
        /// <summary>
        /// Underlying Dummy <see cref="IElementTransformJsApi"/> implementation. Values are populated from Unity's transform.
        /// </summary>
        private class PlayerTransformJsApi : IElementTransformJsApi
        {
            /// <summary>
            /// Cached Unity Transform, used when setting values from the JsApi.
            /// </summary>
            private readonly Transform _unityTransform;

            /// <summary>
            /// Creates a new PlayerTransformJsApi
            /// </summary>
            /// <param name="unityTransform"></param>
            public PlayerTransformJsApi(Transform unityTransform)
            {
                _unityTransform = unityTransform;
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

            /// <summary>
            /// Position.
            /// </summary>
            public Vec3 position {
                get
                {
                    return _position;
                }
                set
                {
                    _position = value;
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //_unityTransform.position = new Vector3(_position.x, _position.y, _position.z);
                }
            }

            /// <summary>
            /// Rotation.
            /// </summary>
            public Quat rotation { 
                get
                {
                    return _rotation;
                }
                set
                {
                    _rotation = value;
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //_unityTransform.rotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
                }
            }

            /// <summary>
            /// Scale.
            /// </summary>
            public Vec3 scale {
                get
                {
                    return _scale;
                }
                set
                {
                    _scale = value;
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //_unityTransform.localScale = new Vector3(_scale.x, _scale.y, _scale.z);
                }
            }

            /// <summary>
            /// Updates <see cref="position"/>, <see cref="rotation"/>, and <see cref="scale"/> to match the underlying Unity Transform's values.
            /// </summary>
            public void UpdateTransform()
            {
                _position.Set(_unityTransform.position.x, _unityTransform.position.y, _unityTransform.position.z);
                _rotation.Set(_unityTransform.rotation.x, _unityTransform.rotation.y, _unityTransform.rotation.z, _unityTransform.rotation.w);
                _scale.Set(_unityTransform.localScale.x, _unityTransform.localScale.y, _unityTransform.localScale.z);
            }
        }

        /// <summary>
        /// Backing PlayerTransformJsApi.
        /// </summary>
        private PlayerTransformJsApi _transform;

        /// <summary>
        /// The transform interface.
        /// </summary>
        public new IElementTransformJsApi transform { get { return _transform; } }

        /// <summary>
        /// Always returns false, since PlayerJs cannot belong to the hierarchy.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool isChildOf(IEntityJs parent)
        {
            return false;
        }

        /// <summary>
        /// Called by Unity. Basic setup.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _transform = new PlayerTransformJsApi(gameObject.transform);
        }

        /// <summary>
        /// Called by Unity. Responsible for syncing Unity's transform with our <see cref="PlayerTransformJsApi"/>.
        /// </summary>
        protected void Update()
        {
            _transform.UpdateTransform();
        }
    }
}