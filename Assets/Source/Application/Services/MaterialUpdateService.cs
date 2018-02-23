using System.Collections.Generic;
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
        /// Manages content.
        /// </summary>
        private readonly IContentManager _content;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MaterialUpdateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IAdminAppDataManager appData,
            IContentManager content)
            : base(binder, messages)
        {
            _appData = appData;
            _content = content;
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

            UpdateContent(material);
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

        /// <summary>
        /// Updates all active content with matching material.
        /// </summary>
        /// <param name="materialData">Material to update.</param>
        private void UpdateContent(MaterialData materialData)
        {
            // find content using this material + update
            var materialId = materialData.Id;
            var matches = new List<ContentWidget>();
            var allContentData = _appData.GetAll<ContentData>();
            for (int i = 0, ilen = allContentData.Length; i < ilen; i++)
            {
                var contentData = allContentData[i];
                if (contentData.MaterialId == materialId)
                {
                    // find active content
                    _content.FindAll(contentData.Id, matches);
                    
                    // send update to all related content
                    var jlen = matches.Count;
                    if (jlen > 0)
                    {
                        Log.Info(this, "Pushing material update to active content.");

                        for (var j = 0; j < jlen; j++)
                        {
                            matches[j].UpdateMaterialData(materialData);
                        }

                        matches.Clear();
                    }
                }
            }
        }
    }
}