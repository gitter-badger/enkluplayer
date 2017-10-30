using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Handles updates for <c>Content</c>.
    /// </summary>
    public class ContentUpdateService : ApplicationService
    {
        /// <summary>
        /// Manages app data.
        /// </summary>
        private readonly IAdminAppDataManager _appData;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentUpdateService(
            IBridge bridge,
            IMessageRouter messages,
            IAdminAppDataManager appData)
            : base(bridge, messages)
        {
            _appData = appData;
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            Subscribe<ContentListEvent>(MessageTypes.CONTENT_LIST, OnContentListEvent);
            Subscribe<ContentAddEvent>(MessageTypes.CONTENT_ADD, OnContentAddEvent);
            Subscribe<ContentRemoveEvent>(MessageTypes.CONTENT_REMOVE, OnContentRemoveEvent);
            Subscribe<ContentUpdateEvent>(MessageTypes.CONTENT_UPDATE, OnContentUpdateEvent);
        }

        /// <summary>
        /// Called with an authoratative list of all content.
        /// 
        /// TODO: Add/Update/Remove
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnContentListEvent(ContentListEvent @event)
        {
            Log.Info(this, "Updating ContentData with {0} instances.", @event.Content.Length);

            _appData.Set(@event.Content);
        }

        /// <summary>
        /// Called when new content has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnContentAddEvent(ContentAddEvent @event)
        {
            Log.Info(this, "Add content.");

            _appData.Add(@event.Content);
        }

        /// <summary>
        /// Called when content has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnContentRemoveEvent(ContentRemoveEvent @event)
        {
            Log.Info(this, "Remove content.");

            var content = _appData.Get<ContentData>(@event.Id);
            if (null != content)
            {
                _appData.Remove(content);
            }
            else
            {
                Log.Warning(this, "Remove content request was for content of unknown id.");
            }
        }

        /// <summary>
        /// Called when content has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnContentUpdateEvent(ContentUpdateEvent @event)
        {
            Log.Info(this, "Update content.");

            // update app data (the hierarchy is listening for updates)
            _appData.Update(@event.Content);
        }
    }
}