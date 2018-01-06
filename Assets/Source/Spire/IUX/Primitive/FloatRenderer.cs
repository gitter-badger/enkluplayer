using System;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Dynamics;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class FloatRenderer : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private WidgetConfig _config;
        private ITweenConfig _tweens;
        private IColorConfig _colors;
        private IIntentionManager _intention;

        /// <summary>
        /// Primitive.
        /// </summary>
        private FloatPrimitive _primitive;

        /// <summary>
        /// Initialization.
        /// </summary>
        internal void Initialize(
            FloatPrimitive primitive,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interaction,
            IInteractableManager interactables)
        {
            _primitive = primitive;
            _tweens = tweens;
            _colors = colors;
            _config = config;
            _intention = intention;
        }

        #region Fields

        /// <summary>
        /// Parent transform
        /// </summary>
        public Widget Parent;

        /// <summary>
        /// Affected Transform
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Transform for Link Anchoring
        /// </summary>
        public Transform LinkTransform;

        /// <summary>
        /// Used for movement
        /// </summary>
        public Magnet Magnet;

        /// <summary>
        /// Ideal offset from camera
        /// </summary>
        public Vector3 Offset = new Vector3(0, 0, 2.5f);

        /// <summary>
        /// Minimum distance for the appearence of this context menu
        /// </summary>
        public float MinimumGroundHeight = 1;

        /// <summary>
        /// FOV Scale
        /// </summary>
        public float FOVScale = 1.2f;

        /// <summary>
        /// Shown when stationary
        /// </summary>
        public Widget StationaryWidget;

        /// <summary>
        /// Shown when in motion
        /// </summary>
        public Widget MotionWidget;

        /// <summary>
        /// Links to the parent menu
        /// </summary>
        public DynamicLink LinkPrefab;

        #endregion

        #region Properties

        /// <summary>
        /// returns the ideal position
        /// </summary>
        public Vector3 IdealPosition
        {
            get
            {
                
                var focusOrigin = _intention.Origin;
                var zAxis = _intention.Forward;
                var xAxis = Vec3.Cross(zAxis, Vec3.Up).Normalized;
                var yAxis = -Vec3.Cross(zAxis, xAxis).Normalized;
                var idealPosition
                    = focusOrigin
                      + xAxis * Offset.x
                      + yAxis * Offset.y
                      + zAxis * Offset.z;
                idealPosition.y
                    = Math.Max(
                        idealPosition.y,
                        MinimumGroundHeight);

                return idealPosition;
            }
        }

        /// <summary>
        /// True if currently moving
        /// </summary>
        public bool IsInMotion
        {
            get
            {
                if (Magnet == null)
                {
                    return false;
                }

                return Magnet.IsInMotion;
            }
        }

        /// <summary>
        /// Returns true if this context menu is visible to user
        /// </summary>
        public bool IsVisibleToUser
        {
            get
            {
                var widgetManager = WidgetManager.Instance;
                if (widgetManager == null)
                {
                    return true;

                }

                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    return false;
                }

                var focusOrigin = widgetManager.FocusOrigin;
                var focusDirection = widgetManager.FocusDirection;

                var delta
                    = Transform
                          .position
                      - focusOrigin;
                var deltaDirection
                    = delta
                        .normalized;
                var lookCosTheta
                    = Vector3
                        .Dot(
                            deltaDirection,
                            focusDirection);
                var lookThetaDegrees
                    = Mathf.Approximately(lookCosTheta, 1.0f)
                        ? 0.0f
                        : Mathf.Acos(lookCosTheta)
                          * Mathf.Rad2Deg;
                var maxLookThetaDegrees
                    = mainCamera.fieldOfView
                      * FOVScale;
                var isLooking
                    = lookThetaDegrees
                      < maxLookThetaDegrees;

                return isLooking;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Frame base initialization
        /// </summary>
        public void Awake()
        {
            if (LinkPrefab != null)
            {
                LinkPrefab.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        public void Update()
        {
            if (Magnet == null)
            {
                return;
            }

            UpdateMovement();
            UpdateWidgets();
            UpdateLink();
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Dynamic Link Instance
        /// </summary>
        private DynamicLink _dynamicLink;

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the movement of the context menu
        /// </summary>
        private void UpdateMovement()
        {
            var idealPosition = IdealPosition;

            if (Magnet.Target.IsEmpty
                || IsInMotion
                || !IsVisibleToUser)
            {
                Magnet
                    .SetTarget(
                        new Target()
                        {
                            Position = idealPosition
                        });
            }
        }

        /// <summary>
        /// Upates widget visibility
        /// </summary>
        private void UpdateWidgets()
        {
            var isStationaryVisible
                = Magnet.IsNearTarget;

            if (StationaryWidget != null)
            {
                StationaryWidget.SetLocalVisible(isStationaryVisible);
            }

            if (MotionWidget != null)
            {
                MotionWidget.SetLocalVisible(!isStationaryVisible);
            }
        }

        /// <summary>
        /// Updates a dynamic link
        /// </summary>
        private void UpdateLink()
        {
            if (LinkPrefab == null)
            {
                return;
            }

            if (Transform == null)
            {
                Transform = transform;
            }

            if (LinkTransform == null)
            {
                LinkTransform = transform;
            }

            if (IsInMotion)
            {
                if (_dynamicLink != null
                    && !_dynamicLink.IsFadingOut)
                {
                    _dynamicLink.FadeOut();
                    _dynamicLink = null;
                }
            }
            else
            {
                if (_dynamicLink == null)
                {
                    _dynamicLink = Instantiate(LinkPrefab);
                    _dynamicLink.transform.SetParent(transform, false);
                    _dynamicLink.gameObject.SetActive(true);
                }
            }

            if (_dynamicLink != null)
            {
                _dynamicLink.EndPoint0
                    = new Target()
                    {
                        Transform = Parent != null ? Parent.transform : null,
                        Position = LinkTransform.position
                    };
                _dynamicLink.EndPoint0.Position.y = 0;

                if (!_dynamicLink.IsFadingOut)
                {
                    _dynamicLink.EndPoint1
                        = new Target()
                        {
                            Transform = null,
                            Position = LinkTransform.position
                        };
                }
            }
        }

        #endregion
    }
}
