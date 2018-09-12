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
        /// Manages elements.
        /// </summary>
        private readonly IElementManager _elements;

        /// <summary>
        /// Updates elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementDelegate;
        
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
            IElementUpdateDelegate elementDelegate,
            IAppSceneManager scenes,
            IDesignController designer,
            IElementManager elements)
            : base(binder, messages)
        {
            _elements = elements;
            _elementDelegate = elementDelegate;
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
            Subscribe<BridgeHelperRefreshElementScriptEvent>(
                MessageTypes.BRIDGE_HELPER_REFRESH_ELEMENT_SCRIPTS,
                OnRefreshElementScripts);

            _binder.Add<BridgeHelperGizmoEvent>(MessageTypes.BRIDGE_HELPER_GIZMO_TRANSLATION);
            _binder.Add<BridgeHelperGizmoEvent>(MessageTypes.BRIDGE_HELPER_GIZMO_ROTATION);
            _binder.Add<BridgeHelperGizmoEvent>(MessageTypes.BRIDGE_HELPER_GIZMO_SCALE);
            _binder.Add<BridgeHelperGizmoEvent>(MessageTypes.BRIDGE_HELPER_TRANSFORM_SPACE);
        }

        /// <summary>
        /// Called on a reparent event.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnReparent(BridgeHelperReparentEvent @event)
        {
            Log.Info(this, "Received Reparent request from bridge.");

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

            _elementDelegate
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
            _designer.Focus(@event.SceneId, @event.ElementId);
        }

        /// <summary>
        /// Called when an element should refresh its scripts.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnRefreshElementScripts(BridgeHelperRefreshElementScriptEvent @event)
        {
            var element = _elements.ById(@event.Id);
            if (null == element)
            {
                Log.Warning(this, "Received refresh event for element that doesn't exist.");
                return;
            }

            var content = element as ContentWidget;
            if (null == content)
            {
                Log.Warning(this,
                    "Received refresh event for {0} element, but only valid for ContentWigets.",
                    element.GetType().Name);
                return;
            }

            content.RefreshScripts();
        }
    }
}