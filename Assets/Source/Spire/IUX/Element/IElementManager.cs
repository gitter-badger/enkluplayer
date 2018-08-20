using System;
using System.Collections.ObjectModel;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Describes an object that manages all elements.
    /// </summary>
    public interface IElementManager
    {
        /// <summary>
        /// Retrieves all elements.
        /// </summary>
        ReadOnlyCollection<Element> All { get; }

        /// <summary>
        /// Adds an element.
        /// </summary>
        /// <param name="element">The element to add.</param>
        void Add(Element element);

        /// <summary>
        /// Invoked when elements are added to <see cref="All"/>
        /// </summary>
        Action<Element> OnAdded { get; }

        /// <summary>
        /// Invoked when elements are removed from <see cref="All"/>
        /// </summary>
        Action<Element> OnRemoved { get; }

        /// <summary>
        /// Retrieves an element by id.
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <returns></returns>
        Element ById(string id);
        
        /// <summary>
        /// Retrieves an element by guid.
        /// </summary>
        /// <param name="guid">Guid of the element.</param>
        /// <returns></returns>
        Element ByGuid(string guid);
    }
}
