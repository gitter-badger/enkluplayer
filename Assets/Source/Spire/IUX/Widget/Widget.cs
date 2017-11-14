using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using System.Diagnostics;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Defines how a widget determines its color
    /// </summary>
    public enum ColorMode
    {
        InheritColor,
        InheritAlpha,
        InheritTween,
        Local
    }

    /// <summary>
    /// Defines how a widget determines its visibility
    /// </summary>
    public enum VisibilityMode
    {
        Inherit,
        Local
    }

    /// <summary>
    /// Base class for IUX elements.
    /// </summary>
    public class Widget : Element, IWidget, ILayerable
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        public ILayerManager Layers { get; private set; }
        public IColorConfig Colors { get; private set; }
        public ITweenConfig Tweens { get; private set; }
        public IPrimitiveFactory Primitives { get; private set; }
        public IWidgetConfig Config { get; private set; }
        public IMessageRouter Messages { get; private set; }

        /// <summary>
        /// True if the widget is currently visible
        /// </summary>
        private readonly WatchedValue<bool> _isVisible = new WatchedValue<bool>();

        /// <summary>
        /// Special code path for first visbility refresh
        /// </summary>
        private bool _firstVisbilityRefresh = true;

        /// <summary>
        /// Layer this widget belongs to (Only root widgets need this set)
        /// </summary>
        private Layer _layer;

        /// <summary>
        /// Tween Value
        /// </summary>
        private float _localTween = 0.0f;

        /// <summary>
        /// True if the widget is currently visible
        /// </summary>
        private bool _localVisible;

        /// <summary>
        /// True if the window has been visible
        /// </summary>
        protected bool _hasBeenVisible;

        /// <summary>
        /// Associated game object
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// Color
        /// </summary>
        public Color LocalColor = Color.white;

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenIn = TweenType.Responsive;

        /// <summary>
        /// Tween type for transitions out
        /// </summary>
        public TweenType TweenOut = TweenType.Responsive;

        /// <summary>
        /// Colorize to a specific color
        /// </summary>
        public VirtualColor VirtualColor = VirtualColor.None;

        /// <summary>
        /// Defines the widget color mode
        /// </summary>
        public ColorMode ColorMode = ColorMode.InheritColor;

        /// <summary>
        /// Default mode is to inherit visibility
        /// </summary>
        public VisibilityMode VisibilityMode = VisibilityMode.Inherit;
        
        /// <summary>
        /// Anchors for children widgets
        /// </summary>
        public WidgetAnchors Anchors;

        /// <summary>
        /// True if should start visible
        /// </summary>
        public bool StartVisible = true;

        /// <summary>
        /// If true, destroys the widget when tween reaches 0
        /// </summary>
        public bool AutoDestroy;
        
        /// <summary>
        /// Controls local GameObject visibility, not parent.
        /// </summary>
        public bool LocalVisible
        {
            get
            {
                return _localVisible;
            }
            set
            {
                if (_localVisible != value)
                {
                    _localVisible = value;

                    UpdateVisibility();
                }

                if (_localVisible)
                {
                    GameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Invoked when Visibility chnages
        /// </summary>
        public WatchedValue<bool> OnVisible
        {
            get { return _isVisible; }
        }

        /// <summary>
        /// Parent accessor
        /// </summary>
        public new Widget Parent
        {
            get { return base.Parent as Widget; }
        }
        
        /// <summary>
        /// Tween for the widget.
        /// </summary>
        public float Tween
        {
            get
            {
                var tween = _localTween;

                if (Parent != null)
                {
                    tween *= Parent.Tween;
                }

                return tween;
            }
        }

        /// <summary>
        /// Color Accessor
        /// </summary>
        public Color Color
        {
            get
            {
                var finalColor = LocalColor;

                finalColor.a *= Tween;

                if (ColorMode == ColorMode.InheritColor)
                {
                    var parentColor = Color.white;
                    if (Parent != null)
                    {
                        parentColor = Parent.Color;
                    }

                    finalColor *= parentColor;
                }

                if (ColorMode == ColorMode.InheritAlpha)
                {
                    var parentAlpha = 1.0f;
                    if (Parent != null)
                    {
                        parentAlpha = Parent.Color.a;
                    }

                    var color = finalColor;
                    color.a *= parentAlpha;
                    return color;
                }

                return finalColor;
            }
        }
        
        /// <summary>
        /// Layer accessor.
        /// </summary>
        public Layer Layer
        {
            get
            {
                if (_layer == null)
                {
                    if (Parent != null)
                    {
                        return Parent.Layer;
                    }
                }

                return _layer;
            }
        }

        /// <summary>
        /// Returns true if the layer is interactive.
        /// </summary>
        public bool LayerInteractive
        {
            get
            {
                var modalLayer = Layers.ModalLayer;
                var layerInteractive = modalLayer == null || modalLayer == Layer;

                return layerInteractive;
            }
        }

        /// <summary>
        /// Is Visible Modification.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible.Value; }
        }

        /// <summary>
        /// Retrieves the transform.
        /// </summary>
        public GameObject GameObject { get { return _gameObject; } }

        /// <summary>
        /// Layer mode.
        /// </summary>
        public LayerMode LayerMode { get; private set; }

        /// <summary>
        /// Initialization
        /// </summary>
        internal void Initialize (
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IPrimitiveFactory primitives,
            IMessageRouter messages)
        {
            Config = config;
            Layers = layers;
            Tweens = tweens;
            Colors = colors;
            Primitives = primitives;
            Messages = messages;
        }

        private ElementSchemaProp<string> _propName;
        private ElementSchemaProp<Vec3> _localPosition;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _propName = Schema.Get<string>("name");
            _localPosition = Schema.Get<Vec3>("localPosition");

            _gameObject = new GameObject(_propName.Value);
            _gameObject.transform.localPosition = _localPosition.Value.ToVector();

            OnVisible.OnChanged += IsVisible_OnUpdate;

            UpdateVisibility();

            if (LayerMode == LayerMode.Modal)
            {
                BringToTop();
            }

            if (StartVisible)
            {
                Show();
            }
        }

        /// <summary>
        /// Invoked when the widget is destroyed
        /// </summary>
        protected override void UnloadInternal()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.Destroy(_gameObject);
                _gameObject = null;
            }

            if (_layer != null)
            {
                Layers.Release(_layer);
            }

            LocalVisible = false;
        }

        /// <summary>
        /// String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Widget[{0}]", GameObject.name);
        }

        /// <summary>
        /// Shows the widget
        /// </summary>
        public void Show()
        {
            LocalVisible = true;
        }

        /// <summary>
        /// Hides the widget
        /// </summary>
        public void Hide()
        {
            LocalVisible = false;
        }
        
        /// <summary>
        /// Brings the layer to the foreground
        /// </summary>
        public void BringToTop()
        {
            LayerMode = LayerMode.Modal;

            if (_layer != null)
            {
                Layers.Release(_layer);
                _layer = null;
            }

            _layer = Layers.Request(this);
        }

        /// <summary>
        /// Updates the visibility chain
        /// </summary>
        /// <param name="visible"></param>
        public void SetLocalVisible(bool visible)
        {
            LocalVisible = visible;
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void UpdateInternal()
        {
            var deltaTime = Time.deltaTime;
            UpdateVisibility();
            UpdateTween(deltaTime);
            UpdateColor(deltaTime);
            UpdateAutoDestroy();
        }
        
        /// <summary>
        /// Updates the visibility
        /// </summary>
        private void UpdateVisibility()
        {
            var parentVisible = VisibilityMode != VisibilityMode.Inherit
                || Parent == null
                || Parent.IsVisible;

            var layerIsVisible = !(LayerMode == LayerMode.Hide && !LayerInteractive);

            var isVisible = LocalVisible && parentVisible && layerIsVisible;
            if (_firstVisbilityRefresh || isVisible != _isVisible.Value)
            {
                _firstVisbilityRefresh = false;
                _isVisible.Value = isVisible;
            }
        }

        /// <summary>
        /// Updates the color of the widget
        /// </summary>
        private void UpdateColor(float deltaTime)
        {
            var virtualColor = VirtualColor;

            if (virtualColor != VirtualColor.None)
            {
                var newColor = Colors.GetColor(virtualColor);
                newColor.a = LocalColor.a;
                LocalColor = IsVisible
                    ? Color.Lerp(LocalColor, newColor, deltaTime * 5.0f)
                    : newColor;
            }
        }

        /// <summary>
        /// Automatic Destruction when visibility drops
        /// </summary>
        private void UpdateAutoDestroy()
        {
            if (AutoDestroy)
            {
                if (_hasBeenVisible
                 && !LocalVisible
                 && Mathf.Approximately(0, Tween))
                {
                    Destroy();
                }
            }
        }
        
        /// <summary>
        /// Updates the fade for the control
        /// </summary>
        private void UpdateTween(float deltaTime)
        {
            var tweenType = IsVisible
                ? TweenIn
                : TweenOut;

            var tweenDuration = Tweens.DurationSeconds(tweenType);
            if (tweenDuration < Mathf.Epsilon)
            {
                _localTween = IsVisible
                    ? 1.0f
                    : 0.0f;
            }
            else
            {
                var multiplier = IsVisible
                    ? 1.0f
                    : -1.0f;
                var tweenDelta = deltaTime / tweenDuration * multiplier;

                _localTween = Mathf.Clamp01(_localTween + tweenDelta);
            }
        }
        
        /// <summary>
        /// Invoked when visibility changes
        /// </summary>
        /// <param name="isVisible"></param>
        private void IsVisible_OnUpdate(bool isVisible)
        {
            LogVerbose(string.Format("Widget Visible[={0}]", isVisible));

            if (isVisible)
            {
                _hasBeenVisible = true;
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
    }
}