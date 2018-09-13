using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Service for updating shader information.
    /// </summary>
    public class ShaderUpdateService : ApplicationService
    {
        /// <summary>
        /// AppData.
        /// </summary>
        private readonly IAdminAppDataManager _appData;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ShaderUpdateService(
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
            base.Start();

            Subscribe<ShaderListEvent>(MessageTypes.SHADER_LIST, Messages_OnListEvent);
            Subscribe<ShaderAddEvent>(MessageTypes.SHADER_ADD, Messages_OnAddEvent);
            Subscribe<ShaderUpdateEvent>(MessageTypes.SHADER_UPDATE, Messages_OnUpdateEvent);
            Subscribe<ShaderRemoveEvent>(MessageTypes.SHADER_REMOVE, Messages_OnRemoveEvent);
        }

        /// <summary>
        /// Called when a complete list of Shaders has been received.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnListEvent(ShaderListEvent @event)
        {
            Log.Info(this, "Set Shader list.");

            _appData.Set(@event.Shaders);
        }

        /// <summary>
        /// Called when a Shader has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAddEvent(ShaderAddEvent @event)
        {
            Log.Info(this, "Add Shader {0}.", @event.Shader);

            _appData.Add(@event.Shader);
        }

        /// <summary>
        /// Called when a Shader has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnUpdateEvent(ShaderUpdateEvent @event)
        {
            Log.Info(this, "Update Shader {0}.", @event.Shader);

            _appData.Update(@event.Shader);
        }

        /// <summary>
        /// Called when a Shader has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnRemoveEvent(ShaderRemoveEvent @event)
        {
            Log.Info(this, "Remove Shader {0}.", @event.Id);

            var data = _appData.Get<ShaderData>(@event.Id);
            _appData.Remove(data);
        }
    }
}