using CreateAR.Commons.Unity.Logging;
using System.Diagnostics;
using CreateAR.SpirePlayer.IUX.Dynamics;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders a <c>Float</c>.
    /// </summary>
    public class FloatRenderer : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IIntentionManager _intention;

        /// <summary>
        /// Primitive.
        /// </summary>
        private Float _float;

        /// <summary>
        /// Dynamic Link Instance
        /// </summary>
        private LinkRenderer _dynamicLink;

        /// <summary>
        /// True if has been stationary
        /// </summary>
        private bool _hasBeenStationary;

        /// <summary>
        /// FOV Scale.
        /// </summary>
        private const float CLOSE_FOV_SCALE = 0.5f;

        /// <summary>
        /// ??
        /// </summary>
        private const float DISTANCE_SCALE = 0.25f;
        
        /// <summary>
        /// Parent Widget.
        /// </summary>
        public Widget Parent;

        /// <summary>
        /// Affected Transform.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Transform for Link Anchoring.
        /// </summary>
        public Transform LinkTransform;

        /// <summary>
        /// Used for movement.
        /// 
        /// TODO: Arguably should not be in a renderer.
        /// </summary>
        public Magnet Magnet;

        /// <summary>
        /// Ideal offset from camera.
        /// </summary>
        public Vector3 Offset = new Vector3(0, 0, 2.5f);

        /// <summary>
        /// Focus position.
        /// </summary>
        public Vector3 Focus = new Vector3(0, 0, 0);

        /// <summary>
        /// Minimum distance for the appearence of this context menu.
        /// </summary>
        public float MinimumGroundHeight = 1;

        /// <summary>
        /// Shown when stationary.
        /// </summary>
        public WidgetRenderer StationaryWidget;

        /// <summary>
        /// Shown when in motion.
        /// </summary>
        public WidgetRenderer MotionWidget;

        /// <summary>
        /// Only active once has been stationary.
        /// </summary>
        public GameObject Trail;

        /// <summary>
        /// Links to the parent menu.
        /// </summary>
        public LinkRenderer LinkPrefab;

        /// <summary>
        /// Focus sphere.
        /// </summary>
        public GameObject FocusSphere;

        /// <summary>
        /// FOV Scale at which to reorient.
        /// </summary>
        public float ReorientFovScale { get; set; }

        /// <summary>
        /// Returns the ideal position.
        /// </summary>
        public Vec3 IdealPosition
        {
            get
            {
                if (_intention == null)
                {
                    return transform.position.ToVec();
                }

                var focusOrigin = _intention.Origin;
                var zAxis = _intention.Forward;
                var xAxis = _intention.Right;
                var yAxis = _intention.Up;
                var idealPosition = focusOrigin
                    + xAxis * Offset.x
                    + yAxis * Offset.y
                    + zAxis * Offset.z;

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
        public bool IsCloseToIdealPosition
        {
            get
            {
                return _intention.IsVisible(
                    transform.position.ToVec(),
                    CLOSE_FOV_SCALE);
            }
        }
        
        /// <summary>
        /// Initialization.
        /// </summary>
        internal void Initialize(
            Float @float,
            IIntentionManager intention)
        {
            _float = @float;
            _intention = intention;

            StationaryWidget.Initialize(_float);
            MotionWidget.Initialize(_float);
        }

        /// <inheritdoc cref="object"/>
        public override string ToString()
        {
            return string.Format("[{0}]", GetType().Name);
        }

        /// <summary>
        /// Clears the link
        /// </summary>
        public void ClearLink()
        {
            if (_dynamicLink != null)
            {
                LogVerbose("Clear DynamicLink.");

                _dynamicLink.FadeOut();
                _dynamicLink = null;
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            if (Magnet == null)
            {
                LogVerbose("Missing Magnet!");
            }

            if (LinkPrefab != null)
            {
                LinkPrefab.gameObject.SetActive(false);
            }

            if (Transform == null)
            {
                Transform = transform;
            }

            if (LinkTransform == null)
            {
                LinkTransform = transform;
            }
        }

        /// <summary>
        /// Called before first Update.
        /// </summary>
        private void Start()
        {
            if (Transform != null)
            {
                Magnet.Root = Magnet.Root.transform.parent;
                Transform = transform.parent;

                var initialPosition = IdealPosition;
                //initialPosition.y = Anchor.FloorY - 2.0f;
                Transform.position = initialPosition.ToVector();

                var trailRenderer = GetComponentInChildren<TrailRenderer>(true);
                if (trailRenderer != null)
                {
                    trailRenderer.Clear();
                }
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (_float.Visible)
            {
                UpdateMovement();
            }

            UpdateWidgets();
            UpdateLink();
            UpdateFocus();
        }

        /// <summary>
        /// Updates the movement of the context menu
        /// </summary>
        private void UpdateMovement()
        {
            var tooFar = CalculateIsTooFarAway();
            var isInFieldOfView = _intention.IsVisible(
                transform.position.ToVec(),
                ReorientFovScale);

            if (Magnet.Target.IsEmpty
                || !isInFieldOfView
                || tooFar)
            {
                if (!IsInMotion
                    || !IsCloseToIdealPosition
                    || tooFar)
                {
                    Log.Warning(this, "!IsInMotion={0}, !IsCloseToIdealPosition={1}, TooFar={2}",
                        !IsInMotion,
                        !IsCloseToIdealPosition,
                        tooFar);

                    MoveToIdealPosition();
                }
            }
        }

        /// <summary>
        /// Upates widget visibility
        /// </summary>
        private void UpdateWidgets()
        {
            var isStationaryVisible = Magnet.IsNearTarget;

            if (StationaryWidget != null)
            {
                StationaryWidget.LocalVisible = isStationaryVisible;
            }

            if (MotionWidget != null)
            {
                MotionWidget.LocalVisible = !isStationaryVisible;
            }

            if (_hasBeenStationary)
            {
                if (Trail != null)
                {
                    Trail.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Updates a dynamic link
        /// </summary>
        private void UpdateLink()
        {
            if (LinkPrefab == null)
            {
                // valid to have no link prefab
                return;
            }

            var linkIsVisible = _float.Visible && !IsInMotion;

            if (linkIsVisible)
            {
                CreateLink();
                _hasBeenStationary = true;
            }
            else
            {
                ClearLink();
            }

            if (_dynamicLink != null)
            {
                _dynamicLink.EndPoint0 = new Target
                {
                    Position = LinkTransform.position
                };
                _dynamicLink.EndPoint0.Position.y = 0; // Anchor.FloorY;

                _dynamicLink.EndPoint1 = new Target
                {
                    Position = LinkTransform.position
                };
            }
        }

        /// <summary>
        /// Updates the focus.
        /// </summary>
        private void UpdateFocus()
        {
            FocusSphere.transform.localPosition = Focus;
        }

        /// <summary>
        /// Sets the internal magnet target to the ideal position.
        /// </summary>
        private void MoveToIdealPosition()
        {
            Magnet.SetTarget(
                new Target
                {
                    Position = IdealPosition.ToVector()
                });
        }

        /// <summary>
        /// Creates the dynamic link
        /// </summary>
        private void CreateLink()
        {
            if (_dynamicLink == null)
            {
                LogVerbose("Create DynamicLink.");

                _dynamicLink = Instantiate(LinkPrefab);
                _dynamicLink.transform.SetParent(transform, false);
                _dynamicLink.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Returns true if this context menu is too far away.
        /// </summary>
        private bool CalculateIsTooFarAway()
        {
            var delta = _intention.Origin - transform.position.ToVec();
            var distance = delta.Magnitude;
            var minDistance = Offset.z * DISTANCE_SCALE;
            var maxDistance = Offset.z * (1.0f + DISTANCE_SCALE);

            if (distance < minDistance || distance > maxDistance)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs verbosely.
        /// </summary>
        /// <param name="message">Verbose logging.</param>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message)
        {
            Log.Info(this, message);
        }
    }
}
