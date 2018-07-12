using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A group of filters, elements, and controllers.
    /// </summary>
    public interface IElementControllerGroup
    {
        /// <summary>
        /// Tags the group with a readable name.
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// True iff the group should be adding/removing controllers.
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Destroys the group.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Adds a filter to narrow down list of elements to affect.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <returns></returns>
        IElementControllerGroup Filter(IElementControllerFilter filter);

        /// <summary>
        /// Removes a filter, which will widen the list of affected elements.
        /// </summary>
        /// <param name="filter">The filter to remove.</param>
        /// <returns></returns>
        IElementControllerGroup Unfilter(IElementControllerFilter filter);

        /// <summary>
        /// Adds a component to all affected elements.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="context">An object to pass to the component.</param>
        /// <returns></returns>
        IElementControllerGroup Add<T>(object context) where T : ElementDesignController;

        /// <summary>
        /// Removes a component from all affected elements.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns></returns>
        IElementControllerGroup Remove<T>() where T : ElementDesignController;

        /// <summary>
        /// Retrieves all active components of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="collection">A list to add components to.</param>
        void All<T>(IList<T> collection) where T : ElementDesignController;
    }
}