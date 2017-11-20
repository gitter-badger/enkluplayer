using System;
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
        /// IElement properties.
        /// </summary>
        public ElementSchema Schema { get { return _widget.Schema; } }
        public IElement[] Children { get { return _widget.Children; } }
        public event Action<IElement, IElement> OnChildRemoved;
        public event Action<IElement, IElement> OnChildAdded;
        public event Action<IElement> OnDestroy;

        /// <summary>
        /// IWidget properties.
        /// </summary>
        public GameObject GameObject { get { return gameObject; } }
        public IWidget Parent { get { return _widget.Parent; } }
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
        
        /// <summary>
        /// Underlying driver that powers the widget monobehavior.
        /// </summary>
        public Widget Widget { get { return _widget; } }

        /// <summary>
        /// Initialization
        /// </summary>
        protected virtual void Awake()
        {
            _widget = new Widget();
        }

        /// <summary>
        /// Widget initialization.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        public void Load(ElementData data, ElementSchema schema, IElement[] children)
        {
            _widget.Load(data, schema, children);
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
