﻿using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
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
        /// Interaction source states.
        /// </summary>
#if UNITY_WSA
        private readonly UnityEngine.XR.WSA.Input.InteractionSourceState[] _interactionSourceStates = new UnityEngine.XR.WSA.Input.InteractionSourceState[10];
#endif

        /// <summary>
        /// Current sample index.
        /// </summary>
        private int _angularVelocityDegreesSampleIndex;

        /// <summary>
        /// Widget which is currently focused
        /// </summary>
        private IInteractable _focusWidget;

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
        public IInteractableManager Interactables { get; set; }

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
        /// Return if the supplied position is visible to the user.
        /// </summary>
        public bool IsVisible(Vec3 position, float fovScale = 1.0f)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            var delta = position - Origin;
            var deltaDirection = delta.ToVector().normalized;
            var lookCosTheta = Vector3.Dot(
                deltaDirection,
                Forward.ToVector());
            var lookThetaDegrees = Mathf.Approximately(lookCosTheta, 1.0f)
                ? 0.0f
                : Mathf.Acos(lookCosTheta) * Mathf.Rad2Deg;
            var maxLookThetaDegrees 
                = mainCamera.fieldOfView 
                * fovScale;

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
            UpdateStability(deltaTime);
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
            Origin = cameraTransform.position.ToVec();
            Forward = cameraTransform.forward.ToVec();
            Up = cameraTransform.up.ToVec();
            Right = cameraTransform.right.ToVec();
        }

        /// <summary>
        /// Updates peripherals.
        /// </summary>
        private void UpdatePeripherals()
        {
            if (Input.GetMouseButton(0))
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                Origin = mouseRay.origin.ToVec();
                Forward = mouseRay.direction.ToVec();
            }
        }

        /// <summary>
        /// Update the hand.
        /// 
        /// TODO: pull out into separate class.
        /// </summary>
        private void UpdateHands()
        {
#if UNITY_WSA
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var cameraTransform = mainCamera.transform;

            var count = UnityEngine.XR.WSA.Input.InteractionManager.GetCurrentReading(_interactionSourceStates);
            for (var i = 0; i < count; ++i)
            {
                Vector3 handPosition;
                var interactionSourceState = _interactionSourceStates[i];
                if (interactionSourceState.source.kind == UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand
                    && interactionSourceState.sourcePose.TryGetPosition(out handPosition))
                {
                    handPosition += cameraTransform.forward * LocalHandOffset.z
                        + cameraTransform.up * LocalHandOffset.y
                        + cameraTransform.right * LocalHandOffset.x;
                    Forward = handPosition.ToVec() - Origin;
                    Forward = Forward.ToVector().normalized.ToVec();

                    break;
                }
            }
#endif
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
    }
}