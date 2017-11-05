using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
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
    public class Widget : InjectableMonoBehaviour, ILayerable, IElement
    {
        /// <summary>
        /// True if the widget is currently visible
        /// </summary>
        private readonly WatchedValue<bool> _isVisible = new WatchedValue<bool>();

        /// <summary>
        /// Cached list of materials
        /// </summary>
        private Material[] _cachedMaterials;

        /// <summary>
        /// Special code path for first visbility refresh
        /// </summary>
        private bool _firstVisbilityRefresh = true;

        /// <summary>
        /// True if the widget is currently focused
        /// </summary>
        private bool _isFocused;

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
        /// Parent of this widget
        /// </summary>
        protected Widget _parent;

        /// <summary>
        /// True if the window has been visible
        /// </summary>
        protected bool _hasBeenVisible;

        /// <summary>
        /// True for first call
        /// </summary>
        protected bool _initialized = false;

        /// <summary>
        /// If true, destroys the widget when tween reaches 0
        /// </summary>
        public bool AutoDestroy;

        /// <summary>
        /// If true, auto generates the buffer collider
        /// </summary>
        public bool AutoGenerateBufferCollider;

        /// <summary>
        /// For losing focus
        /// </summary>
        public BoxCollider BufferCollider;

        /// <summary>
        /// Colorize to a specific color
        /// </summary>
        public VirtualColor Colorized = VirtualColor.None;

        /// <summary>
        /// Defines the widget color mode
        /// </summary>
        public ColorMode ColorMode = ColorMode.InheritColor;

        /// <summary>
        /// For gaining focus
        /// </summary>
        public BoxCollider FocusCollider;

        /// <summary>
        /// Shows/Hides w/ Focus
        /// </summary>
        public Widget ShowIfFocusedWidget;

        /// <summary>
        /// Graphic for the widget
        /// </summary>
        public Graphic Graphic;

        /// <summary>
        /// Integration with IAM
        /// </summary>
        public GameObject InterfaceAnimManagerGameObject;

        /// <summary>
        /// Color
        /// </summary>
        public Color LocalColor = Color.white;

        /// <summary>
        /// Renderer for the widget
        /// </summary>
        public Renderer Renderer;

        /// <summary>
        /// Name of shader color for renderers
        /// </summary>
        public string RendererColorName;

        /// <summary>
        /// True if should start visible
        /// </summary>
        public bool StartsVisible = true;

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenIn = TweenType.Fast;

        /// <summary>
        /// Tween type for transitions out
        /// </summary>
        public TweenType TweenOut = TweenType.Fast;

        /// <summary>
        /// Default mode is to inherit visibility
        /// </summary>
        public VisibilityMode VisibilityMode = VisibilityMode.Inherit;

        /// <summary>
        /// Manages intentions.
        /// </summary>
        [Inject]
        public IntentionManager Intention { get; set; }

        /// <summary>
        /// Manages elements.
        /// </summary>
        [Inject]
        public ElementManager Elements { get; set; }

        /// <summary>
        /// Manages layers.
        /// </summary>
        [Inject]
        public LayerManager Layers { get; set; }

        /// <summary>
        /// Color configuration.
        /// </summary>
        [Inject]
        public ColorConfig Colors { get; set; }

        /// <summary>
        /// App-wide widget configuration.
        /// </summary>
        [Inject]
        public WidgetConfig Config { get; set; }

        /// <summary>
        /// Tween configuration.
        /// </summary>
        [Inject]
        public TweenConfig Tweens { get; set; }

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

                    RefreshIsVisible();
                }

                if (_localVisible)
                {
                    gameObject.SetActive(true);
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
        /// True if the widget is focused
        /// </summary>
        public virtual bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;

                    if (ShowIfFocusedWidget != null)
                    {
                        ShowIfFocusedWidget.LocalVisible = _isFocused;
                    }

                    OnFocus(this, _isFocused);
                }
            }
        }

        /// <summary>
        /// Tween for the widget
        /// </summary>
        public float Tween
        {
            get
            {
                if (!_initialized)
                {
                    _localTween = 0.0f;
                }

                var tween = _localTween;

                if (_parent != null)
                {
                    tween *= _parent.Tween;
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
                    if (_parent != null)
                    {
                        parentColor = _parent.Color;
                    }

                    finalColor *= parentColor;
                }

                if (ColorMode == ColorMode.InheritAlpha)
                {
                    var parentAlpha = 1.0f;
                    if (_parent != null)
                    {
                        parentAlpha = _parent.Color.a;
                    }

                    var color = finalColor;
                    color.a *= parentAlpha;
                    return color;
                }

                return finalColor;
            }
        }

        /// <summary>
        /// Returns the radius of the widget
        /// </summary>
        public virtual float Radius
        {
            get
            {
                var radius = 1f;
                if (null != FocusCollider)
                {
                    var size = FocusCollider.size;
                    var scale = FocusCollider.transform.lossyScale;
                    var scaledSize = new Vector3(
                        size.x * scale.x,
                        size.y * scale.y,
                        size.z * scale.z);
                    radius = (scaledSize.x + scaledSize.y + scaledSize.z) / 3f;
                }

                return radius;
            }
        }

        /// <summary>
        /// Layer accessor
        /// </summary>
        public Layer Layer
        {
            get
            {
                if (_layer == null)
                {
                    if (_parent != null)
                    {
                        return _parent.Layer;
                    }
                }

                return _layer;
            }
        }

        /// <summary>
        /// Returns true if the layer is interactive
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
        /// Is Visible Modification
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible.Value; }
        }

        /// <summary>
        /// Highlight priority.
        /// </summary>
        public int HighlightPriority { get; protected set; }

        /// <summary>
        /// Retrieves the transform.
        /// </summary>
        public Transform Transform { get { return transform; } }

        /// <summary>
        /// Layer mode.
        /// </summary>
        public LayerMode LayerMode { get; private set; }

        /// <summary>
        /// Invoked when focus is gained or lost
        /// 
        /// TODO: Check this out.
        /// </summary>
        public event Action<Widget, bool> OnFocus = delegate { };

        /// <summary>
        /// Invoked when destroyed
        /// </summary>
        public event Action<Widget> OnDestroyed;

        /// <summary>
        /// String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Widget[{0}]", name);
        }

        /// <summary>
        /// Shows the widget
        /// </summary>
        [ContextMenu("Show")]
        public void Show()
        {
            LocalVisible = true;
        }

        /// <summary>
        /// Hides the widget
        /// </summary>
        [ContextMenu("Hide")]
        public void Hide()
        {
            LocalVisible = false;
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        public virtual void Update()
        {
            var deltaTime = Time.smoothDeltaTime;

            FrameUpdate(deltaTime);
        }

        /// <summary>
        /// Invoked when the widget is destroyed
        /// </summary>
        public virtual void OnDestroy()
        {
            if (_layer != null)
            {
                Layers.Release(_layer);
            }

            if (OnDestroyed != null)
            {
                OnDestroyed(this);
            }
            
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
        /// Initializes the widget
        /// </summary>
        public virtual void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            OnVisible.OnChanged += IsVisible_OnUpdate;

            if (_parent == null)
            {
                OnTransformParentChanged();
            }

            InitializeBufferCollider();

            if (LayerMode == LayerMode.Modal)
            {
                BringToTop();
            }

            if (StartsVisible)
            {
                Show();
            }

            _initialized = true;
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
        /// Initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // track this element
            Elements.Add(this);
        }

        /// <summary>
        /// Generate buffer collider
        /// </summary>
        protected void GenerateBufferCollider()
        {
            if (FocusCollider == null)
            {
                Log.Error(this, "Missing FocusCollider for AutoGenerateBufferCollider!");
                return;
            }

            if (BufferCollider == null)
            {
                BufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            BufferCollider.size = FocusCollider.size * Config.AutoGeneratedWidgetBufferColliderSize;

            if (FocusCollider.transform != transform)
            {
                var size = BufferCollider.size;
                size.x *= FocusCollider.transform.localScale.x;
                size.y *= FocusCollider.transform.localScale.y;
                size.z *= FocusCollider.transform.localScale.z;
                BufferCollider.size = size;
            }
        }

        /// <summary>
        /// Initializes the buffer collider for the widget
        /// </summary>
        private void InitializeBufferCollider()
        {
            if (AutoGenerateBufferCollider)
            {
                GenerateBufferCollider();
            }
        }

        /// <summary>
        /// Updates the visibility
        /// </summary>
        private void UpdateVisibility()
        {
            RefreshIsVisible();

            if (ShowIfFocusedWidget != null)
            {
                ShowIfFocusedWidget.LocalVisible = _isFocused;
            }
        }

        /// <summary>
        /// Updates the color of the widget
        /// </summary>
        private void UpdateColor(float deltaTime)
        {
            var colorized = Colorized;

            if (colorized != VirtualColor.None)
            {
                var newColor = Colors.GetColor(colorized);
                newColor.a = LocalColor.a;
                LocalColor = IsVisible
                    ? Color.Lerp(LocalColor, newColor, deltaTime * 5.0f)
                    : newColor;
            }

            if (Graphic != null)
            {
                Graphic.color = Color;
            }

            UpdateRenderers();
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
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Updates the renderer
        /// </summary>
        private void UpdateRenderers()
        {
            if (string.IsNullOrEmpty(RendererColorName))
            {
                return;
            }

            if (Renderer == null)
            {
                Renderer = GetComponent<Renderer>();
            }

            if (Renderer == null)
            {
                Log.Warning(this, "Missing Renderer!");
                return;
            }

            if (_cachedMaterials == null)
            {
                _cachedMaterials = Renderer.materials;
            }

            if (_cachedMaterials.Length > 0)
            {
                var color = Color;
                for (int j = 0, jCount = _cachedMaterials.Length; j < jCount; ++j)
                {
                    var material = _cachedMaterials[j];
                    if (material != null)
                    {
                        material.SetColor(
                            RendererColorName,
                            color);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when children have been changed
        /// </summary>
        private void OnTransformParentChanged()
        {
            FindParentWidget(transform);

            RefreshIsVisible();

            if (_parent != null)
            {
                FrameUpdate(0);
            }
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        /// <param name="deltaTime"></param>
        private void FrameUpdate(float deltaTime)
        {
            Initialize();
            UpdateVisibility();
            UpdateTween(deltaTime);
            UpdateColor(deltaTime);
            UpdateAutoDestroy();
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
        /// Finds the parent transform
        /// </summary>
        /// <param name="child"></param>
        private void FindParentWidget(Transform child)
        {
            var parentTransform = child.parent;
            if (parentTransform == null)
            {
                return;
            }

            var parentWidget = parentTransform.GetComponent<Widget>();
            if (parentWidget != null)
            {
                _parent = parentWidget;
            }
            else
            {
                FindParentWidget(parentTransform);
            }
        }

        /// <summary>
        /// Refreshes the status of the global visibility of this widget
        /// </summary>
        private void RefreshIsVisible()
        {
            var parentVisible = VisibilityMode != VisibilityMode.Inherit
                || _parent == null
                || _parent.IsVisible;

            var layerIsVisible = !(LayerMode == LayerMode.Hide && !LayerInteractive);

            var isVisible = LocalVisible && parentVisible && layerIsVisible;
            if (_firstVisbilityRefresh || isVisible != _isVisible.Value)
            {
                _firstVisbilityRefresh = false;
                _isVisible.Value = isVisible;
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

                // TODO: REMOVE ASAP
                if (InterfaceAnimManagerGameObject != null)
                {
                    InterfaceAnimManagerGameObject.SendMessage("UpdateAnimClips");
                    InterfaceAnimManagerGameObject.SendMessage("startAppear", false,
                        SendMessageOptions.DontRequireReceiver);
                }

                gameObject.SetActive(true);
            }
            else
            {
                if (_hasBeenVisible)
                {
                    // TODO: REMOVE ASAP
                    if (InterfaceAnimManagerGameObject != null)
                    {
                        InterfaceAnimManagerGameObject.SendMessage("startDisappear", false,
                            SendMessageOptions.DontRequireReceiver);
                    }
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