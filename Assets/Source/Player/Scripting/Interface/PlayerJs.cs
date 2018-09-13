using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Represents the user, for JsApi libraries to tie into. Currently not exported for scripts to use.
    /// </summary>
    public class PlayerJs : InjectableMonoBehaviour, IEntityJs
    {
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
        public IGestureManager GestureManager { get; set; }

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
            var handGameObject = new GameObject("Hand");
            handGameObject.transform.SetParent(gameObject.transform);
            hand = new HandJs(handGameObject);

            GestureManager.OnPointerStarted += OnPointerStarted;
            GestureManager.OnPointerEnded += OnPointerEnded;
        }

        /// <summary>
        /// Called by Unity. Cleanup subscriptions.
        /// </summary>
        private void OnDestroy()
        {
            GestureManager.OnPointerStarted -= OnPointerStarted;
            GestureManager.OnPointerEnded -= OnPointerEnded;
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
                GestureManager.TryGetPointerOrigin(_pointerId, out handPosition);

                hand.UpdatePosition(handPosition);
            }
        }

        /// <summary>
        /// Called when a new Gesture is being tracked.
        /// </summary>
        /// <param name="pointerId"></param>
        private void OnPointerStarted(uint pointerId)
        {
            _pointerId = pointerId;
        }

        /// <summary>
        /// Called when a Gesture has lost tracking.
        /// </summary>
        /// <param name="pointerId"></param>
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