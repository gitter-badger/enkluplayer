using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Received hierarchy-related events from the bridge and passed them along
    /// to the database.
    /// </summary>
    public class HierarchyUpdateService : ApplicationService
    {
        /// <summary>
        /// Stores hierarchy data.
        /// </summary>
        private readonly HierarchyDatabase _database;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyUpdateService(
            IBridge bridge,
            IMessageRouter messages,
            HierarchyDatabase database)
            : base(bridge, messages)
        {
            _database = database;
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            // TODO: REMOVE
            _bridge.Binder.Add<HierarchySelectEvent>(MessageTypes.HIERARCHY_SELECT);

            Subscribe<HierarchyListEvent>(MessageTypes.HIERARCHY_LIST, OnHierarchyListEvent);
            Subscribe<HierarchyAddEvent>(MessageTypes.HIERARCHY_ADD, OnHierarchyAddEvent);
            Subscribe<HierarchyRemoveEvent>(MessageTypes.HIERARCHY_REMOVE, OnHierarchyRemoveEvent);
            Subscribe<HierarchyUpdateEvent>(MessageTypes.HIERARCHY_UPDATE, OnHierarchyUpdateEvent);
        }

        /// <summary>
        /// Called with an authoratative list of the entire hierarchy.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyListEvent(HierarchyListEvent @event)
        {
            Log.Info(this, "Hierarchy list updated.");

            _database.Set(@event.Children);
        }

        /// <summary>
        /// Called when a node has been added to the hierarchy.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyAddEvent(HierarchyAddEvent @event)
        {
            Log.Info(this, "Add node to hierarchy.");

            _database.Add(@event.Parent, @event.Node);
        }

        /// <summary>
        /// Called when a node has been removed from the hierarchy.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyRemoveEvent(HierarchyRemoveEvent @event)
        {
            Log.Info(this, "Remove node from hierarchy.");

            _database.Remove(@event.ContentId);
        }

        /// <summary>
        /// Called when a node has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyUpdateEvent(HierarchyUpdateEvent @event)
        {
            Log.Info(this, "Update hierarchy node.");

            _database.Update(@event.Node);
        }
    }
}