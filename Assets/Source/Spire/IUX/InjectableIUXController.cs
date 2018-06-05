using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Base class for IUX controllers.
    /// </summary>
    public class InjectableIUXController : InjectableMonoBehaviour
    {
        /// <summary>
        /// True iff Inject() has already been called.
        /// </summary>
        private bool _isInjected;

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

        /// <inheritdoc />
        protected override void Awake()
        {
            Inject();
        }

        /// <inheritdoc cref="MonoBehaviour" />
        protected virtual void OnDisable()
        {
            Root.Schema.Set("visible", false);
        }

        /// <inheritdoc cref="MonoBehaviour" />
        protected virtual void OnEnable()
        {
            Root.Schema.Set("visible", true);
        }

        /// <inheritdoc cref="MonoBehaviour" />
        protected virtual void OnDestroy()
        {
            if (null != Root)
            {
                Root.Destroy();
            }
        }

        /// <summary>
        /// Processes injection.
        /// </summary>
        protected void Inject()
        {
            // Inject may only be called once.
            if (_isInjected)
            {
                return;
            }

            _isInjected = true;

            Main.Inject(this);

            var attributes = GetType().GetCustomAttributes(typeof(InjectVineAttribute), true);
            if (1 != attributes.Length)
            {
                Log.Error(this, "Could not fine InjectVineAttribute on {0}.", name);
                return;
            }

            var identifier = ((InjectVineAttribute)attributes[0]).Identifier;
            var vine = Vines.Vine(identifier);

            if (null != vine)
            {
                Root = Elements.Element(vine.Text);
                Root.Schema.Set("visible", false);

                var widget = Root as Widget;
                if (null != widget)
                {
                    widget.GameObject.transform.SetParent(transform, false);
                }

                InjectElementsAttribute.InjectElements(this, Root);
            }
            else
            {
                Log.Error(this,
                    "Could not find vine for {0} with identifier {1}.",
                    name,
                    identifier);
            }
        }
    }
}