using CreateAR.Commons.Unity.Logging;
using System.Diagnostics;
using CreateAR.SpirePlayer.IUX.Dynamics;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class FloatRenderer : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
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
            IIntentionManager intention)
        {
            _primitive = primitive;
            _intention = intention;

            StationaryWidget.Initialize(_primitive);
            MotionWidget.Initialize(_primitive);
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
        public const float REORIENT_FOV_SCALE = 1.5f;

        /// <summary>
        /// FOV Scale
        /// </summary>
        public const float CLOSE_FOV_SCALE = 0.5f;

        /// <summary>
        /// Too far away from the origin
        /// </summary>
        public const float DISTANCE_SCALE = 0.25f;

        /// <summary>
        /// Shown when stationary
        /// </summary>
        public WidgetRenderer StationaryWidget;

        /// <summary>
        /// Shown when in motion
        /// </summary>
        public WidgetRenderer MotionWidget;

        /// <summary>
        /// Only active once has been stationary
        /// </summary>
        public GameObject Trail;

        /// <summary>
        /// Links to the parent menu
        /// </summary>
        public LinkRenderer LinkPrefab;

        /// <summary>
        /// returns the ideal position
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
                var idealPosition
                    = focusOrigin
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
        public bool IsInFieldOfView { get { return _intention.IsVisible(transform.position.ToVec(), REORIENT_FOV_SCALE); } }

        /// <summary>
        /// Returns true if this context menu is visible to user
        /// </summary>
        public bool IsTooFarAway
        {
            get
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
        }

        /// <summary>
        /// Returns true if this context menu is visible to user
        /// </summary>
        public bool IsCloseToIdealPosition { get { return _intention.IsVisible(transform.position.ToVec(), CLOSE_FOV_SCALE); } }

        /// <summary>
        /// String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DynamicNode";
        }

        /// <summary>
        /// Frame base initialization
        /// </summary>
        public void Awake()
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
        public void Start()
        {
            if (Transform != null)
            {
                // TODO: fix this, hiearchy of spawned IUX objects is extra deep.
                // TODO: fix this, hiearchy of spawned IUX objects is extra deep.
                Magnet.Root = Magnet.Root.transform.parent.parent;
                Transform = transform.parent.parent;

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

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void Update()
        {
            if (_primitive.Visible)
            {
                UpdateMovement();
            }

            UpdateWidgets();
            UpdateLink();
        }

        /// <summary>
        /// Moves to ideal position
        /// </summary>
        public void MoveToIdealPosition()
        {
            Magnet
                .SetTarget(
                    new Target()
                    {
                        Position = IdealPosition.ToVector()
                    });
        }

        /// <summary>
        /// Dynamic Link Instance
        /// </summary>
        private LinkRenderer _dynamicLink;

        /// <summary>
        /// True if has been stationary
        /// </summary>
        private bool _hasBeenStationary;

        /// <summary>
        /// Updates the movement of the context menu
        /// </summary>
        private void UpdateMovement()
        {
            if (Magnet.Target.IsEmpty
                || !IsInFieldOfView
                || IsTooFarAway)
            {
                if (!IsInMotion || !IsCloseToIdealPosition || IsTooFarAway)
                {
                    MoveToIdealPosition();
                }
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

            var linkIsVisible
                = _primitive.Visible
                  && !IsInMotion;

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
                _dynamicLink.EndPoint0
                    = new Target()
                    {
                        Position = LinkTransform.position
                    };
                _dynamicLink.EndPoint0.Position.y = 0; // Anchor.FloorY;

                _dynamicLink.EndPoint1
                    = new Target()
                    {
                        Position = LinkTransform.position
                    };
            }
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

        /// <summary>
        /// Logs verbosely.
        /// </summary>
        /// <param name="message">Verbose logging.</param>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message)
        {
            Log.Info(this, message);
        }

        #endregion
    }
}
