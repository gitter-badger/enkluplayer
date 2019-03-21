using System.Collections.Generic;
using Enklu.Orchid;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Interface for multiplayer.
    /// </summary>
    [JsInterface("mp")]
    public class MultiplayerJsInterface
    {
        /// <summary>
        /// Lookup from element id to context object.
        /// </summary>
        private readonly Dictionary<string, MultiplayerContextJs> _contexts = new Dictionary<string, MultiplayerContextJs>();

        /// <summary>
        /// The multiplayer controller.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;

        public bool isConnected
        {
            get { return _multiplayer.IsConnected; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerJsInterface(IMultiplayerController multiplayer)
        {
            _multiplayer = multiplayer;
        }

        public MultiplayerContextJs context(ElementJs element)
        {
            MultiplayerContextJs context;
            if (!_contexts.TryGetValue(element.id, out context))
            {
                context = _contexts[element.id] = new MultiplayerContextJs(_multiplayer, element.Element);
            }

            return context;
        }

        public void onConnectionChange(IJsCallback cb)
        {
            _multiplayer.OnConnectionChanged += connected => cb.Apply(this, connected);
        }
    }
}