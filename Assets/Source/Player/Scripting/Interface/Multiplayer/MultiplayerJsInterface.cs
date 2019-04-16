using System.Collections.Generic;
using Enklu.Mycelium.Messages.Experience;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Interface for multiplayer.
    /// </summary>
    [JsInterface("mp")]
    public class MultiplayerJsInterface
    {
        /// <summary>
        /// Expiration types.
        /// </summary>
        public class ExpirationTypes
        {
            public static readonly string SESSION = ElementExpirationType.Session.ToString();
            public static readonly string PERSISTENT = ElementExpirationType.Persistent.ToString();
        }

        /// <summary>
        /// Ownership types.
        /// </summary>
        public class OwnershipTypes
        {
            public static readonly string SELF = "self";
        }

        /// <summary>
        /// Lookup from element id to context object.
        /// </summary>
        private readonly Dictionary<string, MultiplayerContextJs> _contexts = new Dictionary<string, MultiplayerContextJs>();

        /// <summary>
        /// The multiplayer controller.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;

        /// <summary>
        /// Cached js wrappers.
        /// </summary>
        private readonly IElementJsCache _elements;

        /// <summary>
        /// Configuration for application.
        /// </summary>
        private readonly ApplicationConfig _config;

        public readonly ExpirationTypes expiration = new ExpirationTypes();

        public readonly OwnershipTypes ownership = new OwnershipTypes();

        public bool isConnected
        {
            get { return _multiplayer.IsConnected; }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerJsInterface(
            IMultiplayerController multiplayer,
            IElementJsCache elements,
            ApplicationConfig config)
        {
            _multiplayer = multiplayer;
            _elements = elements;
            _config = config;
        }

        public MultiplayerContextJs context(ElementJs element)
        {
            MultiplayerContextJs context;
            if (!_contexts.TryGetValue(element.id, out context))
            {
                context = _contexts[element.id] = new MultiplayerContextJs(
                    _multiplayer,
                    _elements,
                    _config,
                    element.Element);
            }

            return context;
        }

        public void onConnectionChange(IJsCallback cb)
        {
            _multiplayer.OnConnectionChanged += connected => cb.Apply(this, connected);
        }
    }
}