using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// IUXController.
    /// </summary>
    public class MonoBehaviourIUXController : MonoBehaviourUIElement
    {
        /// <summary>
        /// Reference to vine.
        /// </summary>
        private VineReference _reference;

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

        /// <inheritdoc />
        public override void Added()
        {
            base.Added();

            if (null == Root)
            {
                return;
            }

            Root.Schema.Set("visible", true);
        }

        /// <summary>
        /// Called after elements have been created.
        /// </summary>
        protected virtual void AfterElementsCreated()
        {

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

        /// <inheritdoc cref="MonoBehaviour" />
        private void Awake()
        {
            Main.Inject(this);

            _reference = Vines.Vine(VineId);
            if (null == _reference)
            {
                return;
            }

            _reference.OnUpdated += Vine_OnUpdated;

            CreateElements();
            AfterElementsCreated();
        }

        /// <summary>
        /// Creates elements from vine.
        /// </summary>
        private void CreateElements()
        {
            var visible = false;
            if (null != Root)
            {
                visible = Root.Schema.Get<bool>("visible").Value;
                Root.Destroy();
            }

            Element el;
            try
            {
                el = Elements.Element(_reference.Text);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not create elements from Vine : {0}.", exception);
                return;
            }
            
            Root = el;
            Root.Schema.Set("visible", visible);

            var widget = Root as Widget;
            if (null != widget)
            {
                widget.GameObject.transform.SetParent(transform, false);
            }

            InjectElementsAttribute.InjectElements(this, Root);
        }

        /// <summary>
        /// Called when the vine has been updated.
        /// </summary>
        private void Vine_OnUpdated()
        {
            CreateElements();
            AfterElementsCreated();
        }
    }
}