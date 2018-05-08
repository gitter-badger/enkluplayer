using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// IUXController.
    /// </summary>
    public class MonoBehaviourIUXController : MonoBehaviourUIElement
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; set; }

        /// <summary>
        /// Finds vines.
        /// </summary>
        [Inject]
        public IVineTable Vines { get; set; }

        /// <summary>
        /// Root element created.
        /// </summary>
        public Element Root { get; private set; }

        /// <summary>
        /// Vine.
        /// </summary>
        public string VineId;

        /// <inheritdoc cref="MonoBehaviour" />
        protected virtual void Awake()
        {
            Main.Inject(this);

            var reference = Vines.Vine(VineId);
            if (null == reference)
            {
                return;
            }

            try
            {
                Root = Elements.Element(reference.Text);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not create elements from Vine : {0}.", exception);
                return;
            }
            
            Root.Schema.Set("visible", false);

            var widget = Root as Widget;
            if (null != widget)
            {
                widget.GameObject.transform.SetParent(transform, false);
            }

            InjectElementsAttribute.InjectElements(this, Root);
        }

        /// <inheritdoc />
        public override void Removed()
        {
            base.Removed();

            if (null == Root)
            {
                return;
            }

            Root.Schema.Set("visible", false);
        }

        /// <inheritdoc />
        public override void Added()
        {
            base.Removed();

            if (null == Root)
            {
                return;
            }

            Root.Schema.Set("visible", true);
        }
        
        /// <inheritdoc cref="MonoBehaviour" />
        protected virtual void OnDestroy()
        {
            if (null == Root)
            {
                return;
            }

            Root.Destroy();
        }
    }
}