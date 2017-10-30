using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages <c>ContentGraph</c> related events.
    /// </summary>
    public class ContentGraphUpdateService : ApplicationHostService
    {
        /// <summary>
        /// The <c>ContentGraph</c>.
        /// </summary>
        private readonly ContentGraph _graph;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentGraphUpdateService(
            IBridge bridge,
            IMessageRouter messages,
            ContentGraph graph)
            : base(bridge, messages)
        {
            _graph = graph;
        }

        /// <inheritdoc cref="ApplicationHostService"/>
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

            _graph.Add(_graph.Root.Id, @event.Children);

            _graph.Walk(node => { Silly("\t{0}", node); });
        }

        /// <summary>
        /// Called when a node has been added to the hierarchy.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyAddEvent(HierarchyAddEvent @event)
        {
            Log.Info(this, "Add node to hierarchy.");

            _graph.Add(@event.Parent, @event.Node);
        }

        /// <summary>
        /// Called when a node has been removed from the hierarchy.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyRemoveEvent(HierarchyRemoveEvent @event)
        {
            Log.Info(this, "Remove node from hierarchy.");

            _graph.Remove(@event.ContentId);
        }

        /// <summary>
        /// Called when a node has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnHierarchyUpdateEvent(HierarchyUpdateEvent @event)
        {
            _graph.Update(@event.Node);
        }
    }
}