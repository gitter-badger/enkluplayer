using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Base class for IUX elements.
    /// </summary>
    public class Widget : Element, ILayerable
    {
        /// <summary>
        /// Regex for parsing element names.
        /// </summary>
        private readonly Regex _elementParser = new Regex(@"Guid=([a-zA-Z0-9\-]+)");

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
        /// Current tween Value.
        /// </summary>
        private float _localTween = 0.0f;

        /// <summary>
        /// True if the widget is currently visible.
        /// </summary>
        private bool _localVisible;

        /// <summary>
        /// Component that controls Widget facing direction.
        /// </summary>
        private FaceComponent _faceComponent;
        
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Col4> _localColorProp;
        private ElementSchemaProp<Vec3> _localPositionProp;
        private ElementSchemaProp<TweenType> _tweenInProp;
        private ElementSchemaProp<TweenType> _tweenOutProp;
        private ElementSchemaProp<string> _virtualColorProp;
        private ElementSchemaProp<WidgetColorMode> _colorModeProp;
        private ElementSchemaProp<WidgetVisibilityMode> _visibilityModeProp;
        private ElementSchemaProp<LayerMode> _layerModeProp;
        private ElementSchemaProp<bool> _autoDestroyProp;
        private ElementSchemaProp<string> _faceProp;

        /// <summary>
        /// Cached virtual color.
        /// </summary>
        private VirtualColor _virtualColor;

        /// <summary>
        /// Widget hierarchy.
        /// </summary>
        internal Widget _parent;

        /// <summary>
        /// True if the window has been visible
        /// </summary>
        protected bool _hasBeenVisible;

        /// <summary>
        /// Dependencies.
        /// 
        /// TODO: Switch to protected.
        /// </summary>
        public ILayerManager Layers { get; private set; }
        public ColorConfig Colors { get; private set; }
        public TweenConfig Tweens { get; private set; }
        public WidgetConfig Config { get; private set; }
        public IMessageRouter Messages { get; private set; }

        /// <summary>
        /// True iff <c>LoadInternal</c> has been called.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Color accessor.
        /// </summary>
        public Col4 LocalColor
        {
            get { return _localColorProp.Value; }
            set { _localColorProp.Value = value; }
        }

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenIn
        {
            get { return _tweenInProp.Value; }
            set { _tweenInProp.Value = value; }
        }

        /// <summary>
        /// Tween type for transitions in
        /// </summary>
        public TweenType TweenOut
        {
            get { return _tweenOutProp.Value; }
            set { _tweenOutProp.Value = value; }
        }

        /// <summary>
        /// Colorize to a specific color
        /// </summary>
        public VirtualColor VirtualColor
        {
            get
            {
                return _virtualColor;
            }
            set
            {
                _virtualColorProp.Value = value.ToString();
            }
        }

        /// <summary>
        /// Defines the widget color mode
        /// </summary>
        public WidgetColorMode ColorMode
        {
            get { return _colorModeProp.Value; }
            set { _colorModeProp.Value = value; }
        }

        /// <summary>
        /// Default mode is to inherit visibility
        /// </summary>
        public WidgetVisibilityMode VisibilityMode
        {
            get { return _visibilityModeProp.Value; }
            set { _visibilityModeProp.Value = value; }
        }

        /// <summary>
        /// Layer mode.
        /// </summary>
        public LayerMode LayerMode
        {
            get { return _layerModeProp.Value; }
            set { _layerModeProp.Value = value; }
        }

        /// <summary>
        /// If true, destroys the widget when tween reaches 0
        /// </summary>
        public bool AutoDestroy
        {
            get { return _autoDestroyProp.Value; }
            set { _autoDestroyProp.Value = value; }
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
        /// Tween for the widget.
        /// </summary>
        public float Tween
        {
            get
            {
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
        public Col4 Color
        {
            get
            {
                var finalColor = LocalColor;

                finalColor.a *= Tween;

                if (ColorMode == WidgetColorMode.InheritColor)
                {
                    var parentColor = Col4.White;
                    if (_parent != null)
                    {
                        parentColor = _parent.Color;
                    }

                    finalColor *= parentColor;
                }

                if (ColorMode == WidgetColorMode.InheritAlpha)
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
        /// Layer accessor.
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
        public GameObject GameObject { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public Widget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages)
        {
            if (null == gameObject)
            {
                throw new ArgumentException("GameObject cannot be null.");
            }

            GameObject = gameObject;

            Config = config;
            Layers = layers;
            Tweens = tweens;
            Colors = colors;
            Messages = messages;
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
        /// Initialization
        /// </summary>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _localColorProp = Schema.GetOwn("color", Col4.White);
            _localPositionProp = Schema.GetOwn("position", Vec3.Zero);
            _localPositionProp.OnChanged += LocalPosition_OnChanged;
            _tweenInProp = Schema.GetOwn("tweenIn", TweenType.Responsive);
            _tweenOutProp = Schema.GetOwn("tweenOut", TweenType.Responsive);
            _virtualColorProp = Schema.GetOwn("virtualColor", "None");
            _virtualColorProp.OnChanged += VirtualColor_OnChanged;
            _colorModeProp = Schema.GetOwn("colorMode", WidgetColorMode.InheritColor);
            _visibilityModeProp = Schema.GetOwn("visibilityMode", WidgetVisibilityMode.Inherit);
            _layerModeProp = Schema.GetOwn("layerMode", LayerMode.Default);
            _autoDestroyProp = Schema.GetOwn("autoDestroy", false);
            _faceProp = Schema.GetOwn("face", string.Empty);
            _faceProp.OnChanged += Face_OnChanged;
            UpdateFace(_faceProp.Value);

            GameObject.name = Schema.GetOwn("name", ToString()).Value;
            GameObject.transform.localPosition = _localPositionProp.Value.ToVector();

            InitializeWidgetComponents(GameObject.transform);

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

            IsLoaded = true;
        }

        /// <summary>
        /// Invoked when the widget is destroyed
        /// </summary>
        protected override void AfterUnloadChildrenInternal()
        {
            IsLoaded = false;
            
            _localPositionProp.OnChanged -= LocalPosition_OnChanged;
            _virtualColorProp.OnChanged -= VirtualColor_OnChanged;
            _faceProp.OnChanged -= Face_OnChanged;
            
            Object.Destroy(GameObject);
            GameObject = null;

            if (_layer != null)
            {
                Layers.Release(_layer);
            }

            base.AfterUnloadChildrenInternal();
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            var child = element as Widget;
            if (child != null)
            {
                child._parent = this;
                child.GameObject.transform.SetParent(
                    GetChildHierarchyParent(child),
                    false);
            }
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void UpdateInternal()
        {
            //var deltaTime = Time.smoothDeltaTime;
            var deltaTime = Time.deltaTime;
            UpdateVisibility();
            UpdateTween(deltaTime);
            UpdateColor(deltaTime);
            UpdateAutoDestroy();
        }

        /// <summary>
        /// Retrives the Unity hierarchy root for children.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        protected virtual Transform GetChildHierarchyParent(Widget child)
        {
            return GameObject.transform;
        }
        
        /// <summary>
        /// Initializes all <c>IWidgetComponent</c> children.
        /// </summary>
        private void InitializeWidgetComponents(Transform transform)
        {
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (_elementParser.IsMatch(child.name))
                {
                    continue;
                }

                var components = child.GetComponents<IWidgetComponent>();
                for (int j = 0, jlen = components.Length; j < jlen; j++)
                {
                    components[j].Widget = this;
                }
            }
        }

        /// <summary>
        /// Updates the visibility
        /// </summary>
        private void UpdateVisibility()
        {
            var parentVisible = VisibilityMode != WidgetVisibilityMode.Inherit
                || _parent == null
                || _parent.Visible;

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

                if (_localTween < Mathf.Epsilon)
                {
                    LogVerbose("My _localTween had been set to zero from visibility.");
                }
            }
            else
            {
                var multiplier = Visible
                    ? 1.0f
                    : -1.0f;
                var tweenDelta = deltaTime / tweenDuration * multiplier;

                _localTween = Mathf.Clamp01(_localTween + tweenDelta);

                if (_localTween < Mathf.Epsilon)
                {
                    LogVerbose("My _localTween had been set to zero from tweening");
                }
            }
        }

        /// <summary>
        /// Updates the facing direction.
        /// </summary>
        /// <param name="facePropValue">Facing direction.</param>
        private void UpdateFace(string facePropValue)
        {
            if (string.IsNullOrEmpty(facePropValue))
            {
                if (null != _faceComponent)
                {
                    Object.Destroy(_faceComponent);
                    _faceComponent = null;
                }

                return;
            }

            if (null == _faceComponent)
            {
                _faceComponent = GameObject.AddComponent<FaceComponent>();
            }

            _faceComponent.Face(facePropValue);
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
        /// Called when the local position changes.
        /// </summary>
        /// <param name="prop">Local position prop.</param>
        /// <param name="prev">Previous position.</param>
        /// <param name="next">Next position.</param>
        private void LocalPosition_OnChanged(
            ElementSchemaProp<Vec3> prop,
            Vec3 prev,
            Vec3 next)
        {
            GameObject.transform.localPosition = next.ToVector();
        }

        /// <summary>
        /// Called when the virtual color changes via schema.
        /// </summary>
        /// <param name="prop">Prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void VirtualColor_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            try
            {
                _virtualColor = (VirtualColor) Enum.Parse(
                    typeof(VirtualColor),
                    next);
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Called when the face changes.
        /// </summary>
        /// <param name="prop">Face prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Face_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateFace(next);
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