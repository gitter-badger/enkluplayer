using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Service for updating material information.
    /// </summary>
    public class MaterialUpdateService : ApplicationService
    {
        /// <summary>
        /// AppData.
        /// </summary>
        private readonly IAdminAppDataManager _appData;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MaterialUpdateService(
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
            
            Subscribe<MaterialListEvent>(MessageTypes.MATERIAL_LIST, Messages_OnListEvent);
            Subscribe<MaterialAddEvent>(MessageTypes.MATERIAL_ADD, Messages_OnAddEvent);
            Subscribe<MaterialUpdateEvent>(MessageTypes.MATERIAL_UPDATE, Messages_OnUpdateEvent);
            Subscribe<MaterialRemoveEvent>(MessageTypes.MATERIAL_REMOVE, Messages_OnRemoveEvent);
        }

        /// <summary>
        /// Called when a complete list of materials has been received.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnListEvent(MaterialListEvent @event)
        {
            Log.Info(this, "Set material list.");

            // set app id
            _appData.Set(@event.Materials);
        }

        /// <summary>
        /// Called when a material has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAddEvent(MaterialAddEvent @event)
        {
            var material = @event.Material;

            Log.Info(this, "Add Material {0}.", material);

            _appData.Add(material);
        }

        /// <summary>
        /// Called when a material has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnUpdateEvent(MaterialUpdateEvent @event)
        {
            var material = @event.Material;

            Log.Info(this, "Update Material {0}.", material);

            _appData.Update(material);
        }

        /// <summary>
        /// Called when a material has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnRemoveEvent(MaterialRemoveEvent @event)
        {
            Log.Info(this, "Remove Material {0}.", @event.Id);

            var data = _appData.Get<MaterialData>(@event.Id);
            _appData.Remove(data);
        }
    }
}