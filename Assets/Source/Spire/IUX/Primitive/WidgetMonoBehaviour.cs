using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    public class WidgetMonoBehaviour : MonoBehaviour, IWidget
    {
        /// <summary>
        /// Internal driver.
        /// </summary>
        private Widget _widget;

        /// <summary>
        /// IElement interface.
        /// </summary>
        public ElementSchema Schema { get { return _widget.Schema; } }
        public IElement[] Children { get { return _widget.Children; } }
        public event Action<IElement, IElement> OnChildRemoved;
        public event Action<IElement, IElement> OnChildAdded;
        public event Action<IElement> OnDestroy;
        public void AddChild(IElement child) { _widget.AddChild(child); }
        public bool RemoveChild(IElement child) { return _widget.RemoveChild(child); }
        public virtual void Load(ElementData data, ElementSchema schema, IElement[] children) { _widget.Load(data, schema, children); }
        public virtual void FrameUpdate() { _widget.FrameUpdate(); }
        public virtual void LateFrameUpdate() { _widget.LateFrameUpdate(); }
        public IElement FindOne(string query) { return _widget.FindOne(query); }
        public string ToTreeString() {  return _widget.ToTreeString(); }

        /// <summary>
        /// IWidget interface.
        /// </summary>
        public GameObject GameObject { get { return gameObject; } }
        public Col4 Color { get { return _widget.Color; } }
        public bool Visible { get { return _widget.Visible; } }
        public float Tween { get { return _widget.Tween; } }
        public Layer Layer { get { return _widget.Layer; } }
        public string Id { get { return _widget.Id; } }
        public Col4 LocalColor
        {
            get { return _widget.LocalColor; }
            set { _widget.LocalColor = value; }
        }
        public bool LocalVisible
        {
            get { return _widget.LocalVisible; }
            set { _widget.LocalVisible = value; }
        }
        public IWidget Parent
        {
            get { return _widget.Parent; }
            set { _widget.Parent = value; }
        }

        /// <summary>
        /// Underlying driver that powers the widget monobehavior.
        /// </summary>
        public Widget Widget { get { return _widget; } }

        /// <summary>
        /// Initializes the widget.
        /// </summary>
        /// <param name="widget"></param>
        public void SetWidget(Widget widget)
        {
            _widget = widget; 
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages)
        {
            _widget = new Widget(gameObject);
            _widget.Initialize(config, layers, tweens, colors, messages);
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void Update()
        {
            // empty, IElement's should override 'UpdateInternal'
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void LateUpdate()
        {
            // empty, IElement's should override 'LateUpdateInternal'
        }
    }
}
