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
        private class UnityTransformJsApi : IElementTransformJsApi
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
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //UnityTransform.position = new Vector3(_position.x, _position.y, _position.z);
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
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //UnityTransform.rotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
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
                    Log.Warning(this, "Attempting to set PlayerJs transform");
                    //UnityTransform.localScale = new Vector3(_scale.x, _scale.y, _scale.z);
                }
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
        }

        /// <summary>
        /// Surfaces properties that are specific to the user's hand.
        /// </summary>
        public class HandJs : IEntityJs
        {
            /// <summary>
            /// Backing transform helper.
            /// </summary>
            private readonly UnityTransformJsApi _unityTransform;

            /// <summary>
            /// Backing GameObject for the Hand.
            /// </summary>
            public readonly GameObject gameObject;

            /// <summary>
            /// Transform.
            /// </summary>
            public IElementTransformJsApi transform
            {
                get { return _unityTransform; }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="gameObject"></param>
            public HandJs(GameObject gameObject)
            {
                this.gameObject = gameObject;
                _unityTransform = new UnityTransformJsApi(gameObject.transform);
            }

            /// <summary>
            /// Always returns false, since the Hand can't belong to the hierarchy.
            /// </summary>
            /// <param name="parent"></param>
            /// <returns></returns>
            public bool isChildOf(IEntityJs parent)
            {
                return false;
            }

            /// <summary>
            /// Sets the transforms local position and updates the JS transform values.
            /// </summary>
            /// <param name="position"></param>
            public void UpdatePosition(Vector3 position)
            {
                _unityTransform.UnityTransform.localPosition = position;
                _unityTransform.UpdateJsTransform();
            }
        }

        /// <summary>
        /// Backing UnityTransformJsApi.
        /// </summary>
        private UnityTransformJsApi _unityTransform;

        /// <summary>
        /// Current pointer ID, if any.
        /// </summary>
        private uint _pointerId;

        /// <summary>
        /// The transform interface.
        /// </summary>
        public new IElementTransformJsApi transform { get { return _unityTransform; } }

        public HandJs hand { get; private set; }

        /// <summary>
        /// Used to place our underlying hand object.
        /// </summary>
        [Inject]
        [DenyJsAccess]
        public IGestureManager _gestureManager { get; set; }

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
            _unityTransform = new UnityTransformJsApi(gameObject.transform);

            // Create child GameObject for hand
            GameObject handGameObject = new GameObject("Hand");
            handGameObject.transform.SetParent(gameObject.transform);
            hand = new HandJs(handGameObject);

            _gestureManager.OnPointerStarted += OnPointerStarted;
            _gestureManager.OnPointerEnded += OnPointerEnded;
        }

        /// <summary>
        /// Called by Unity. Responsible for syncing Unity's transform with our <see cref="UnityTransformJsApi"/>.
        /// </summary>
        protected void Update()
        {
            _unityTransform.UpdateJsTransform();

            if (_pointerId > 0)
            {
                Vector3 handPosition;
                _gestureManager.TryGetPointerOrigin(_pointerId, out handPosition);

                hand.UpdatePosition(handPosition);
            }
        }

        private void OnPointerStarted(uint pointerId)
        {
            _pointerId = pointerId;
        }

        private void OnPointerEnded(uint pointerId)
        {
            if (pointerId == _pointerId)
            {
                _pointerId = 0;

                hand.UpdatePosition(Vector3.zero);
            }
        }
    }
}