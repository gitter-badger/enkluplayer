using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Tracks intention of the user, i.e. view direction and what they are
    /// intending by their view direction.
    /// </summary>
    public class IntentionManager : InjectableMonoBehaviour, IIntentionManager
    {
        /// <summary>
        /// Window for calculating steadiness.
        /// </summary>
        private readonly float[] _angularVelocityDegreesSamples = new float[10];

        /// <summary>
        /// Forward from last frame.
        /// </summary>
        private Vector3 _lastForward = Vector3.forward;

        /// <summary>
        /// Current sample index.
        /// </summary>
        private int _angularVelocityDegreesSampleIndex;

        /// <summary>
        /// Widget which is currently focused
        /// </summary>
        private IInteractable _focusWidget;

        /// <summary>
        /// Last state of mouse down.
        /// </summary>
        private Vec3 _lastMouseOrigin;

        /// <summary>
        /// Last state of mouse do
        /// </summary>
        private Vec3 _lastMouseForward;

        /// <summary>
        /// Camera to use.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// For aiming with a hand.
        /// </summary>
        public Vector3 LocalHandOffset;

        /// <summary>
        /// Maximum angular velocity.
        /// </summary>
        public float MaxAngularVelocity = 40.0f;
        
        /// <summary>
        /// Returns steadiness of the view.
        /// </summary>
        public float Stability { get; private set; }
        
        /// <summary>
        /// Manages interactable objects.
        /// </summary>
        [Inject]
        public IInteractionManager Interactables { get; set; }

        /// <summary>
        /// Main camera tag.
        /// </summary>
        [Inject]
        public MainCamera Main { get; set; }

        /// <summary>
        /// Current focus.
        /// </summary>
        public IInteractable Focus
        {
            get { return _focusWidget; }
            set
            {
                if (_focusWidget != value)
                {
                    if (_focusWidget != null)
                    {
                        _focusWidget.Focused = false;
                    }

                    _focusWidget = value;

                    if (_focusWidget != null)
                    {
                        _focusWidget.Focused = true;
                    }
                }
            }
        }

        /// <summary>
        /// Forward direction.
        /// </summary>
        public Vec3 Forward { get; private set; }

        /// <summary>
        /// Up direction.
        /// </summary>
        public Vec3 Up { get; private set; }

        /// <summary>
        /// Right direction.
        /// </summary>
        public Vec3 Right { get; private set; }

        /// <summary>
        /// Focus origin.
        /// </summary>
        public Vec3 Origin { get; private set; }

        /// <summary>
        /// Focus ray.
        /// </summary>
        public Ray Ray { get { return new Ray(Origin.ToVector(), Forward.ToVector()); } }

        /// <summary>
        /// Tracks average angular velocity in degrees.
        /// </summary>
        public float AverageAngularVelocity { get; private set; }

        /// <summary>
        /// Forward direction.
        /// </summary>
        public float FieldOfView { get; private set; }

        /// <summary>
        /// Return if the supplied position is visible to the user.
        /// </summary>
        public bool IsVisible(Vec3 position, float fovScale = 1.0f)
        {
            var delta = position - Origin;
            var deltaDirection = delta.ToVector().normalized;
            var lookCosTheta = Vector3.Dot(
                deltaDirection,
                Forward.ToVector());
            var lookThetaDegrees = Mathf.Approximately(lookCosTheta, 1.0f)
                ? 0.0f
                : Mathf.Acos(lookCosTheta) * Mathf.Rad2Deg;
            var maxLookThetaDegrees 
                = _camera.fieldOfView 
                * fovScale;

            var isLooking = lookThetaDegrees < maxLookThetaDegrees;
            return isLooking;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected override void Awake()
        {
            base.Awake();

            _camera = Main.GetComponent<Camera>();

            var camTransform = Main.transform;
            _lastMouseOrigin = Origin = camTransform.position.ToVec();
            _lastMouseForward = Forward = camTransform.forward.ToVec();
            Right = camTransform.right.ToVec();
            Up = camTransform.up.ToVec();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Start()
        {
            Interactables.OnRemoved += Interactables_OnRemoved;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            
            UpdatePerspective();
            UpdateMouse();
            UpdateStability(deltaTime);
            UpdateFocus();
        }

        /// <summary>
        /// Update camera properties.
        /// </summary>
        private void UpdatePerspective()
        {
            var cameraTransform = Main.transform;
            Origin = cameraTransform.position.ToVec();
            Forward = cameraTransform.forward.ToVec();
            Up = cameraTransform.up.ToVec();
            Right = cameraTransform.right.ToVec();
        }
        
        /// <summary>
        /// Updates peripherals.
        /// </summary>
        private void UpdateMouse()
        {
            if (!UnityEngine.Application.isEditor
                && UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

                _lastMouseOrigin = Origin = mouseRay.origin.ToVec();
                _lastMouseForward = Forward = mouseRay.direction.ToVec();
            }
            else
            {
                Origin = _lastMouseOrigin;
                Forward = _lastMouseForward;
            }
        }
        
        /// <summary>
        /// Updates the steadiness of the intention.
        /// </summary>
        private void UpdateStability(float deltaTime)
        {
            if (!Mathf.Approximately(deltaTime, 0))
            {
                var deltaCosTheta = Vector3.Dot(Forward.ToVector(), _lastForward);
                var deltaThetaRadians = Mathf.Approximately(deltaCosTheta, 1.0f)
                    ? 0.0f
                    : Mathf.Acos(deltaCosTheta);
                var deltaThetaDegrees = Mathf.Rad2Deg * deltaThetaRadians;
                var angularVelocityDegrees = deltaThetaDegrees / deltaTime;
                _angularVelocityDegreesSamples[_angularVelocityDegreesSampleIndex++] = angularVelocityDegrees;

                if (_angularVelocityDegreesSampleIndex >= _angularVelocityDegreesSamples.Length)
                {
                    _angularVelocityDegreesSampleIndex = 0;
                }

                _lastForward = Forward.ToVector();
            }

            var count = _angularVelocityDegreesSamples.Length;
            var totalAngularVelocity = 0.0f;
            for (var i = 0; i < count; ++i)
            {
                var angularVelocityDegrees = _angularVelocityDegreesSamples[i];
                totalAngularVelocity += angularVelocityDegrees;
            }

            AverageAngularVelocity = totalAngularVelocity / count;
            Stability = 1.0f - Mathf.Clamp01(AverageAngularVelocity / MaxAngularVelocity);
        }
        
        /// <summary>
        /// Updates the current IFocusable that is considered focused.
        /// </summary>
        private void UpdateFocus()
        {
            var all = Interactables.All;
            for (int i = 0, len = all.Count; i < len; i++)
            {
                var interactable = all[i];
                if (interactable.Interactable && interactable.Raycast(Origin, Forward))
                {
                    Focus = interactable;
                    return;
                }
            }
            
            // determine if the current interactable should lose focuse
            if (Focus != null)
            {
                if (!Focus.Raycast(Origin, Forward))
                {
                    Focus = null;
                }
            }
        }

        /// <summary>
        /// Called when an interactable has been removed.
        /// </summary>
        /// <param name="interactable">The interactable in question.</param>
        private void Interactables_OnRemoved(IInteractable interactable)
        {
            if (Focus == interactable)
            {
                Focus = null;
            }
        }
    }
}