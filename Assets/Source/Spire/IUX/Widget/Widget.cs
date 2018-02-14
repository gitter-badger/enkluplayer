using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Base class for IUX elements.
    /// </summary>
    public class Widget : Element, ILayerable, IUnityElement
    {
        /// <summary>
        /// Layer this widget belongs to (Only root widgets need this set).
        /// </summary>
        private Layer _layer;

        /// <summary>
        /// Current tween Value.
        /// </summary>
        private float _localTween = 0.0f;
        
        /// <summary>
        /// Component that controls Widget facing direction.
        /// </summary>
        private FaceComponent _faceComponent;

        /// <summary>
        /// Parent widget, updated every frame.
        /// </summary>
        private Widget _parentWidget;
        
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Col4> _localColorProp;
        private ElementSchemaProp<Vec3> _localPositionProp;
        private ElementSchemaProp<Vec3> _localScaleProp;
        private ElementSchemaProp<bool> _localVisibleProp;
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
        /// Dependencies.
        /// 
        /// TODO: Switch to protected.
        /// </summary>
        [Obsolete]
        public ILayerManager Layers { get; private set; }
        [Obsolete]
        public ColorConfig Colors { get; private set; }
        [Obsolete]
        public TweenConfig Tweens { get; private set; }
        [Obsolete]
        public WidgetConfig Config { get; private set; }
        [Obsolete]
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
        /// Controls local GameObject visibility, not parent.
        /// </summary>
        public bool LocalVisible
        {
            get
            {
                return _localVisibleProp.Value;
            }
            set
            {
                _localVisibleProp.Value = value;
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

                if (_parentWidget != null)
                {
                    tween *= _parentWidget.Tween;
                }

                return tween;
            }
        }

        /// <summary>
        /// Calculated color, based on WidgetColorMode.
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
                    if (_parentWidget != null)
                    {
                        parentColor = _parentWidget.Color;
                    }

                    finalColor *= parentColor;
                }

                if (ColorMode == WidgetColorMode.InheritAlpha)
                {
                    var parentAlpha = 1.0f;
                    if (_parentWidget != null)
                    {
                        parentAlpha = _parentWidget.Color.a;
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
                    if (_parentWidget != null)
                    {
                        return _parentWidget.Layer;
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
        /// True iff the widget is currently visible. This refers to _global_
        /// visibility, not local, i.e. the value is calculated from its parent.
        /// </summary>
        public bool Visible { get; private set; }

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
        /// Initialization
        /// </summary>
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // default to gameobject
            Visible = GameObject.activeSelf;

            _localVisibleProp = Schema.GetOwn("visible", true);
            _localVisibleProp.OnChanged += LocalVisible_OnChanged;
            _localColorProp = Schema.GetOwn("color", Col4.White);
            _localPositionProp = Schema.GetOwn("position", Vec3.Zero);
            _localPositionProp.OnChanged += LocalPosition_OnChanged;
            _localScaleProp = Schema.GetOwn("scale", Vec3.One);
            _localScaleProp.OnChanged += LocalScale_OnChanged;
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
            GameObject.transform.localScale = _localScaleProp.Value.ToVector();
            
            UpdateGlobalVisibility();

            if (LayerMode == LayerMode.Modal)
            {
                BringToTop();
            }
            
            IsLoaded = true;
        }

        /// <summary>
        /// Invoked when the widget is destroyed
        /// </summary>
        protected override void UnloadInternalAfterChildren()
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

            base.UnloadInternalAfterChildren();
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            var child = element as IUnityElement;
            if (child != null)
            {
                child.GameObject.transform.SetParent(
                    GetChildHierarchyParent(element),
                    false);
            }
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void UpdateInternal()
        {
            _parentWidget = GetWidgetParent();

            var deltaTime = Time.deltaTime;

            UpdateGlobalVisibility();
            UpdateTween(deltaTime);
            UpdateColor(deltaTime);
            UpdateAutoDestroy();
        }

        /// <summary>
        /// Retrives the Unity hierarchy root for children.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        protected virtual Transform GetChildHierarchyParent(Element child)
        {
            return GameObject.transform;
        }

        /// <summary>
        /// Retrieves the first ancestor Widget.
        /// </summary>
        /// <returns></returns>
        protected Widget GetWidgetParent()
        {
            var parent = Parent;
            while (null != parent)
            {
                var widget = parent as Widget;
                if (null != widget)
                {
                    return widget;
                }

                parent = parent.Parent;
            }

            return null;
        }
        
        /// <summary>
        /// Updates global visibility.
        /// </summary>
        private void UpdateGlobalVisibility()
        {
            if (VisibilityMode == WidgetVisibilityMode.Local)
            {
                // no change
                if (_localVisibleProp.Value == Visible)
                {
                    return;
                }

                Visible = _localVisibleProp.Value;
            }
            else
            {
                if (!_localVisibleProp.Value)
                {
                    // no change
                    if (Visible == false)
                    {
                        return;
                    }

                    Visible = false;
                }
                // calculate from parent chain
                else
                {
                    var invisibleParent = false;
                    var parent = Parent;
                    while (null != parent)
                    {
                        var widget = parent as Widget;
                        if (null == widget)
                        {
                            var val = parent.Schema.Get<bool>("visible").Value;
                            if (!val)
                            {
                                invisibleParent = true;
                                break;
                            }
                        }
                        else
                        {
                            if (!widget.Visible)
                            {
                                invisibleParent = true;
                                break;
                            }
                        }

                        parent = parent.Parent;
                    }

                    // no change
                    if (Visible != invisibleParent)
                    {
                        return;
                    }

                    Visible = !invisibleParent;
                }
            }

            // guaranteed that visibility changed here
            GameObject.SetActive(Visible);
            
            // push to children
            PushVisibilityUpdateToNearestWidgets(this);

            // call overrideable method _after_ children are updated
            OnVisibilityUpdated();
        }

        /// <summary>
        /// True iff visibility was updated. This method is called after children
        /// update their visibilities.
        /// </summary>
        protected virtual void OnVisibilityUpdated()
        {

        }

        /// <summary>
        /// Pushes a visibility update down into the nearest child Widgets. This
        /// means that if a child is not a Widget, it keeps recursing until it
        /// finds one to pass the update to. This is so that visibility updates
        /// may pass through intermediate widgets if necessary.
        /// </summary>
        /// <param name="element">The element to start with.</param>
        private void PushVisibilityUpdateToNearestWidgets(Element element)
        {
            for (int i = 0, len = element.Children.Count; i < len; i++)
            {
                var child = element.Children[i];

                var widget = child as Widget;
                if (null != widget)
                {
                    widget.UpdateGlobalVisibility();
                }
                else
                {
                    PushVisibilityUpdateToNearestWidgets(child);
                }
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
                if (!LocalVisible
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
        /// Called when the local scale changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous scale.</param>
        /// <param name="next">Next scale.</param>
        private void LocalScale_OnChanged(
            ElementSchemaProp<Vec3> prop,
            Vec3 prev,
            Vec3 next)
        {
            GameObject.transform.localScale = next.ToVector();
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
            _virtualColor = EnumExtensions.Parse<VirtualColor>(next);
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
        /// Called when the local visibility changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void LocalVisible_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateGlobalVisibility();
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