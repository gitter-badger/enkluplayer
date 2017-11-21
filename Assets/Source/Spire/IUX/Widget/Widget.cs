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
        /// Layer this widget belongs to (Only root widgets need this set).
        /// </summary>
        private Layer _layer;

        /// <summary>
        /// Widget hierarchy.
        /// </summary>
        private IWidget _parent;

        /// <summary>
        /// Current tween Value.
        /// </summary>
        private float _localTween = 0.0f;

        /// <summary>
        /// True if the widget is currently visible.
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
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _name;
        private ElementSchemaProp<Col4> _localColor;
        private ElementSchemaProp<Vec3> _localPosition;
        private ElementSchemaProp<TweenType> _tweenIn;
        private ElementSchemaProp<TweenType> _tweenOut;
        private ElementSchemaProp<VirtualColor> _virtualColor;
        private ElementSchemaProp<ColorMode> _colorMode;
        private ElementSchemaProp<VisibilityMode> _visibilityMode;
        private ElementSchemaProp<LayerMode> _layerMode;
        private ElementSchemaProp<bool> _autoDestroy;

        /// <summary>
        /// Color accessor.
        /// </summary>
        public Col4 LocalColor
        {
            get { return _localColor.Value; }
            set { _localColor.Value = value; }
        }

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenIn
        {
            get { return _tweenIn.Value; }
            set { _tweenIn.Value = value; }
        }

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenOut
        {
            get { return _tweenOut.Value; }
            set { _tweenOut.Value = value; }
        }

        /// <summary>
        /// Colorize to a specific color
        /// </summary>
        public VirtualColor VirtualColor
        {
            get { return _virtualColor.Value; }
            set { _virtualColor.Value = value; }
        }

        /// <summary>
        /// Defines the widget color mode
        /// </summary>
        public ColorMode ColorMode
        {
            get { return _colorMode.Value; }
            set { _colorMode.Value = value; }
        }

        /// <summary>
        /// Default mode is to inherit visibility
        /// </summary>
        public VisibilityMode VisibilityMode
        {
            get { return _visibilityMode.Value; }
            set { _visibilityMode.Value = value; }
        }

        /// <summary>
        /// Layer mode.
        /// </summary>
        public LayerMode LayerMode
        {
            get { return _layerMode.Value; }
            set { _layerMode.Value = value; }
        }

        /// <summary>
        /// If true, destroys the widget when tween reaches 0
        /// </summary>
        public bool AutoDestroy
        {
            get { return _autoDestroy.Value; }
            set { _autoDestroy.Value = value; }
        }

        /// <summary>
        /// True if should start visible
        /// </summary>
        public bool StartVisible = true;
        
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
        public IWidget Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                {
                    GameObject.transform.SetParent(null);
                }

                _parent = value;

                if (_parent != null)
                {
                    GameObject
                        .transform
                        .SetParent(_parent.GameObject.transform, false);
                }
            }
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
        public Col4 Color
        {
            get
            {
                var finalColor = LocalColor;

                finalColor.a *= Tween;

                if (ColorMode == ColorMode.InheritColor)
                {
                    var parentColor = Col4.White;
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
        public bool Visible
        {
            get { return _isVisible.Value; }
        }

        /// <summary>
        /// Retrieves the transform.
        /// </summary>
        public GameObject GameObject { get { return _gameObject; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gameObject"></param>
        public Widget(GameObject gameObject = null)
        {
            _gameObject = gameObject ?? new GameObject("Widget");
        }

        /// <summary>
        /// Initialization
        /// </summary>
        internal void Initialize (
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages)
        {
            Config = config;
            Layers = layers;
            Tweens = tweens;
            Colors = colors;
            Messages = messages;

            OnChildAdded += Element_OnChildAdded;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _name = Schema.Get<string>("name");
            _localColor = Schema.Get<Col4>("color");
            _localPosition = Schema.Get<Vec3>("position");

            _tweenIn = Schema.Get<TweenType>("tweenIn");
            _tweenOut = Schema.Get<TweenType>("tweenOut");
            _virtualColor = Schema.Get<VirtualColor>("virtualColor");
            _colorMode = Schema.Get<ColorMode>("colorMode");
            _visibilityMode = Schema.Get<VisibilityMode>("visibilityMode");
            _layerMode = Schema.Get<LayerMode>("layerMode");
            _autoDestroy = Schema.Get<bool>("autoDestroy");

            if (!string.IsNullOrEmpty(_name.Value))
            {
                _gameObject.name = _name.Value;
            }
            _gameObject.transform.localPosition = _localPosition.Value.ToVector();

            for (int i = 0; i < Children.Length; ++i)
            {
                var child = Children[i] as Widget;
                if (child != null)
                {
                    child.GameObject.transform.SetParent(_gameObject.transform, false);
                }
            }

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
            var deltaTime = Time.smoothDeltaTime;
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
                || Parent.Visible;

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
                LocalColor = Visible
                    ? Col4.Lerp(LocalColor, newColor, deltaTime * 5.0f)
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
            var tweenType = Visible
                ? TweenIn
                : TweenOut;

            var tweenDuration = Tweens.DurationSeconds(tweenType);
            if (tweenDuration < Mathf.Epsilon)
            {
                _localTween = Visible
                    ? 1.0f
                    : 0.0f;
            }
            else
            {
                var multiplier = Visible
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
        /// Invoked when a child is added
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        private void Element_OnChildAdded(IElement parent, IElement child)
        {
            if (parent == this)
            {
                var childWidget = child as IWidget;
                if (childWidget != null)
                {
                    childWidget.Parent = this;
                }
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