using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    [Obsolete("DO NOT use. Only here for Activator at the moment.")]
    public class WidgetMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Internal driver.
        /// </summary>
        private Widget _widget;
        
        /// <summary>
        /// IWidget interface.
        /// </summary>
        public GameObject GameObject { get { return gameObject; } }
        public Col4 Color { get { return _widget.Color; } }
        public bool Visible { get { return _widget.Visible; } }

        public float Tween
        {
            get
            {
                return _widget.Tween;
            }
        }

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
        /// Called on awake.
        /// </summary>
        private void Awake()
        {
            _widget = new Widget(gameObject);
        }

        private bool _TEMP_DEBUG = false;

        /// <summary>
        /// Loads for activator.
        /// TODO: This code path should be removed.
        /// </summary>
        /// <param name="parent"></param>
        public void LoadFromActivator(ActivatorPrimitive parent)
        {
            _TEMP_DEBUG = true;

            _widget.Load(new ElementData(), new ElementSchema(), new IElement[0]);
            parent.AddChild(_widget);

            InitializeWidgetRenderers();
        }
        
        /// <summary>
        /// TEMP.
        /// 
        /// Initializes renderers.
        /// </summary>
        private void InitializeWidgetRenderers()
        {
            foreach (var wRenderer in GetComponentsInChildren<WidgetRenderer>())
            {
                wRenderer.Initialize(_widget);
            }
        }
    }
}
