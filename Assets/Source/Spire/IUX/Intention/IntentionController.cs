using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace CreateAR.Spire
{
    /// <summary>
    /// Tracks intention of the user, i.e. view direction and what they are
    /// intending by their view direction.
    /// </summary>
    public class IntentionController : MonoBehaviour
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
        /// Interaction source states.
        /// </summary>
        private readonly InteractionSourceState[] _interactionSourceStates = new InteractionSourceState[10];

        /// <summary>
        /// Current sample index.
        /// </summary>
        private int _angularVelocityDegreesSampleIndex;

        /// <summary>
        /// Backing variables for directional properties.
        /// </summary>
        private Vector3 _forward = Vector3.forward;
        private Vector3 _up = Vector3.up;
        private Vector3 _right = Vector3.right;

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
        public float Steadiness;

        /// <summary>
        /// If true, input is not updated.
        /// </summary>
        public bool InputDisabled { get; private set; }

        /// <summary>
        /// Current focus.
        /// </summary>
        public IFocusable Focus { get; private set; }

        /// <summary>
        /// Forward direction.
        /// </summary>
        public Vector3 Forward
        {
            get { return _forward; }
            private set { _forward = value; }
        }

        /// <summary>
        /// Up direction.
        /// </summary>
        public Vector3 Up
        {
            get { return _up; }
            private set { _up = value; }
        }

        /// <summary>
        /// Right direction.
        /// </summary>
        public Vector3 Right
        {
            get { return _right; }
            private set { _right = value; }
        }

        /// <summary>
        /// Focus origin.
        /// </summary>
        public Vector3 Origin { get; private set; }

        /// <summary>
        /// Focus ray.
        /// </summary>
        public Ray Ray { get { return new Ray(Origin, Forward); } }

        /// <summary>
        /// Tracks average angular velocity in degrees.
        /// </summary>
        public float AverageAngularVelocity { get; private set; }

        /// <summary>
        /// Return if the supplied position is visible to the user.
        /// </summary>
        public bool IsVisible(Vector3 position, float fovScale = 1.0f)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            var delta = position - Origin;
            var deltaDirection = delta.normalized;
            var lookCosTheta = Vector3.Dot(
                deltaDirection,
                Forward);
            var lookThetaDegrees = Mathf.Approximately(lookCosTheta, 1.0f)
                ? 0.0f
                : Mathf.Acos(lookCosTheta) * Mathf.Rad2Deg;
            var maxLookThetaDegrees = mainCamera.fieldOfView * fovScale;

            var isLooking = lookThetaDegrees < maxLookThetaDegrees;
            return isLooking;
        }

        /// <summary>
        /// Updates the focus direction.
        /// </summary>
        public void Update()
        {
            var deltaTime = Time.deltaTime;

            UpdatePerspective();
            UpdatePeripherals();
            UpdateHands();
            UpdateSteadiness(deltaTime);
            UpdateFocus();
        }

        /// <summary>
        /// Update camera properties.
        /// </summary>
        private void UpdatePerspective()
        {
            var perspectiveCamera = Camera.main;
            if (perspectiveCamera == null)
            {
                return;
            }

            var cameraTransform = perspectiveCamera.transform;
            Origin = cameraTransform.position;
            Forward = cameraTransform.forward;
            Up = cameraTransform.up;
            Right = cameraTransform.right;
        }

        /// <summary>
        /// Updates peripherals.
        /// </summary>
        private void UpdatePeripherals()
        {
            if (Input.GetMouseButton(0))
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                Origin = mouseRay.origin;
                Forward = mouseRay.direction;
            }
        }

        /// <summary>
        /// Update the hand.
        /// </summary>
        private void UpdateHands()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var cameraTransform = mainCamera.transform;

            var count = InteractionManager.GetCurrentReading(_interactionSourceStates);
            for (var i = 0; i < count; ++i)
            {
                Vector3 handPosition;
                var interactionSourceState = _interactionSourceStates[i];
                if (interactionSourceState.source.kind == InteractionSourceKind.Hand
                    && interactionSourceState.properties.location.TryGetPosition(out handPosition))
                {
                    handPosition += cameraTransform.forward * LocalHandOffset.z
                        + cameraTransform.up * LocalHandOffset.y
                        + cameraTransform.right * LocalHandOffset.x;
                    Forward = handPosition - Origin;
                    Forward.Normalize();

                    break;
                }
            }
        }

        /// <summary>
        /// Updates the steadiness of the intention.
        /// </summary>
        private void UpdateSteadiness(float deltaTime)
        {
            if (!Mathf.Approximately(deltaTime, 0))
            {
                var deltaCosTheta = Vector3.Dot(Forward, _lastForward);
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

                _lastForward = Forward;
            }

            var count = _angularVelocityDegreesSamples.Length;
            var totalAngularVelocity = 0.0f;
            for (var i = 0; i < count; ++i)
            {
                var angularVelocityDegrees = _angularVelocityDegreesSamples[i];
                totalAngularVelocity += angularVelocityDegrees;
            }

            AverageAngularVelocity = totalAngularVelocity / count;
            Steadiness = 1.0f - Mathf.Clamp01(AverageAngularVelocity / MaxAngularVelocity);
        }

        /// <summary>
        /// Updates the current IFocusable that is considered focused.
        /// </summary>
        private void UpdateFocus()
        {
            if (InputDisabled)
            {
                Focus = null;
                return;
            }

            var layerMask = 1 << LayerMask.NameToLayer(LayerMaskNames.UI);

            RaycastHit raycastHit;
            if (Physics.Raycast(
                Origin,
                Forward,
                out raycastHit,
                Mathf.Infinity,
                layerMask))
            {
                if (raycastHit.collider != null)
                {
                    var focusable = raycastHit
                        .collider
                        .GetComponent<IFocusable>();
                    if (focusable != null
                        && focusable.IsVisible
                        && focusable.FocusCollider != null
                        && focusable.FocusCollider.enabled)
                    {
                        Focus = focusable;
                        return;
                    }
                }
            }

            // determine if the current IFocusable should lose focus
            if (Focus != null)
            {
                if (Focus.UnfocusCollider == null)
                {
                    Focus = null;
                }
                else
                {
                    if (!Focus
                        .UnfocusCollider
                        .Raycast(
                            Ray,
                            out raycastHit,
                            Mathf.Infinity))
                    {
                        Focus = null;
                    }
                }
            }
        }
    }
}