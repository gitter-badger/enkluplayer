using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Service for element actions that the editor can't perform.
    /// </summary>
    public class ElementActionHelperService : ApplicationService
    {
        /// <summary>
        /// Updates elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elements;
        
        /// <summary>
        /// Scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Designer.
        /// </summary>
        private readonly IDesignController _designer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementActionHelperService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IElementUpdateDelegate elements,
            IAppSceneManager scenes,
            IDesignController designer)
            : base(binder, messages)
        {
            _elements = elements;
            _scenes = scenes;
            _designer = designer;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            Subscribe<BridgeHelperReparentEvent>(
                MessageTypes.BRIDGE_HELPER_REPARENT,
                OnReparent);
            Subscribe<BridgeHelperSelectEvent>(
                MessageTypes.BRIDGE_HELPER_SELECT,
                OnSelect);
            Subscribe<BridgeHelperSelectEvent>(
                MessageTypes.BRIDGE_HELPER_FOCUS,
                OnFocus);
        }

        /// <summary>
        /// Called on a reparent event.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnReparent(BridgeHelperReparentEvent @event)
        {
            // find element + parent
            var root = _scenes.Root(@event.SceneId);
            if (null == root)
            {
                Log.Info(this, "Could not find scene root for reparent. SceneId = {0}", @event.SceneId);
                return;
            }

            var element = root.FindOne<Element>(".." + @event.ElementId);
            if (null == element)
            {
                Log.Info(this, "Could not find element for reparent. ElementId = {0}", @event.ElementId);
                return;
            }

            var parent = @event.ParentId == root.Id
                ? root
                : root.FindOne<Element>(".." + @event.ParentId);
            if (null == parent)
            {
                Log.Info(this, "Could not find parent for reparent. ParentId = {0}", @event.ParentId);
                return;
            }

            _elements
                .Reparent(element, parent)
                .OnFailure(exception => Log.Error(this, "Could not reparent : {0}.", exception));
        }

        /// <summary>
        /// Called on select.
        /// </summary>
        /// <param name="event">Select event.</param>
        private void OnSelect(BridgeHelperSelectEvent @event)
        {
            _designer.Select(@event.SceneId, @event.ElementId);
        }

        /// <summary>
        /// Called on focus.
        /// </summary>
        /// <param name="event">Focus.</param>
        private void OnFocus(BridgeHelperSelectEvent @event)
        {

        }
    }
}