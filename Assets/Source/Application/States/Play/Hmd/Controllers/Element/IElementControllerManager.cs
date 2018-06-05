using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// This object tracks Elements using filters, and manages the resulting
    /// elements' components.
    /// </summary>
    public interface IElementControllerManager
    {
        /// <summary>
        /// True iff the controller manager is actively tracking elements.
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Adds a filter to narrow down list of elements to affect.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <returns></returns>
        IElementControllerManager Filter(IElementControllerFilter filter);

        /// <summary>
        /// Removes a filter, which will widen the list of affected elements.
        /// </summary>
        /// <param name="filter">The filter to remove.</param>
        /// <returns></returns>
        IElementControllerManager Unfilter(IElementControllerFilter filter);

        /// <summary>
        /// Adds a component to all affected elements.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="context">An object to pass to the component.</param>
        /// <returns></returns>
        IElementControllerManager Add<T>(object context) where T : ElementDesignController;

        /// <summary>
        /// Removes a component from all affected elements.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns></returns>
        IElementControllerManager Remove<T>() where T : ElementDesignController;

        /// <summary>
        /// Retrieves all active components of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="collection">A list to add components to.</param>
        void All<T>(IList<T> collection) where T : ElementDesignController;
    }
}