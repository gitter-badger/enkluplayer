using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    [Obsolete("DO NOT use. Only here for Activator at the moment.")]
    public class WidgetMonoBehaviour : MonoBehaviour, IWidget
    {
        /// <summary>
        /// Internal driver.
        /// </summary>
        private Widget _widget;

        /// <summary>
        /// TODO: the monobehavior update flow needs refactor, this is gross.
        /// </summary>
        private bool _unityUpdate;

        /// <summary>
        /// IElement interface.
        /// </summary>
        public ElementSchema Schema { get { return _widget.Schema; } }
        public IElement[] Children { get { return _widget.Children; } }
        public event Action<IElement> OnRemoved;
        public event Action<IElement, IElement> OnChildRemoved;
        public event Action<IElement, IElement> OnChildAdded;
        public event Action<IElement> OnDestroyed;
        public void AddChild(IElement child) { _widget.AddChild(child); }
        public bool RemoveChild(IElement child) { return _widget.RemoveChild(child); }
        public virtual void Load(ElementData data, ElementSchema schema, IElement[] children) { _widget.Load(data, schema, children); }
        public virtual void FrameUpdate() { _widget.FrameUpdate(); }
        public virtual void LateFrameUpdate() { _widget.LateFrameUpdate(); }
        public T FindOne<T>(string query) where T : IElement { return _widget.FindOne<T>(query); }
        public void Find(string query, IList<IElement> results) { _widget.Find(query, results); }

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
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages)
        {
            _widget = new Widget(gameObject);
            _widget.Initialize(config, layers, tweens, colors, messages);
        }

        /// <summary>
        /// Loads through the Unity Hierarchy.
        /// TODO: This code path should be removed once we can transition
        /// all primitives to data.
        /// </summary>
        /// <param name="parent"></param>
        public void LoadFromMonoBehaviour(WidgetMonoBehaviour parent)
        {
            var newWidget = new Widget(gameObject);

            SetWidget(newWidget);

            Initialize(
                parent.Widget.Config, 
                parent.Widget.Layers, 
                parent.Widget.Tweens, 
                parent.Widget.Colors, 
                parent.Widget.Messages);

            var data = new ElementData()
            {
                Id = gameObject.name,
            };

            Load(data, new ElementSchema(), new IElement[] { });

            parent.AddChild(this);

            Parent = parent;

            _unityUpdate = true;
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void Update()
        {
            if (_unityUpdate)
            {
                FrameUpdate();
            }
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void LateUpdate()
        {
            if (_unityUpdate)
            {
                LateFrameUpdate();
            }
        }
    }
}
