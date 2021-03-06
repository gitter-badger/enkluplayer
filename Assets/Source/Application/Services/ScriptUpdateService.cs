using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Handles <c>Script</c> updates.
    /// </summary>
    public class ScriptUpdateService : ApplicationService
    {
        /// <summary>
        /// Manages application data.
        /// </summary>
        private readonly IAdminAppDataManager _appData;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptUpdateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IAdminAppDataManager appData)
            : base(binder, messages)
        {
            _appData = appData;
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            Subscribe<ScriptListEvent>(MessageTypes.RECV_SCRIPT_LIST, OnScriptListEvent);
            Subscribe<ScriptAddEvent>(MessageTypes.RECV_SCRIPT_ADD, OnScriptAddEvent);
            Subscribe<ScriptUpdateEvent>(MessageTypes.RECV_SCRIPT_UPDATE, OnScriptUpdateEvent);
            Subscribe<ScriptRemoveEvent>(MessageTypes.RECV_SCRIPT_REMOVE, OnScriptRemoveEvent);
        }

        /// <summary>
        /// Called with an authoritative list of all scripts.
        /// 
        /// TODO: Add/Update/Remove
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnScriptListEvent(ScriptListEvent @event)
        {
            Log.Info(this, "Script list updated.");

            foreach (var script in @event.Scripts)
            {
                Silly("\t-{0}", script);

                _appData.Add(script);
            }
        }

        /// <summary>
        /// Called when a script has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnScriptAddEvent(ScriptAddEvent @event)
        {
            Log.Info(this, "Script added.");

            var script = @event.Script;

            Silly("\t-Script: {0}", script);

            _appData.Add(script);
        }

        /// <summary>
        /// Called when a script has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnScriptUpdateEvent(ScriptUpdateEvent @event)
        {
            Log.Info(this, "Script updated : {0}.", @event.Script);

            _appData.Update(@event.Script);
        }

        /// <summary>
        /// Called when a script has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnScriptRemoveEvent(ScriptRemoveEvent @event)
        {
            Log.Info(this, "Script removed.");

            var data = _appData.Get<ScriptData>(@event.Id);
            if (null == data)
            {
                Log.Warning(this, "Received ScriptRemoveEvent for unknown script id {0}.", @event.Id);
                return;
            }

            _appData.Remove(data);
        }
    }
}