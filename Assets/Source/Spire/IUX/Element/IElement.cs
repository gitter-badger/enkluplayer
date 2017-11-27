using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    public interface IElement
    {
        /// <summary>
        /// Unique id stored in data for this element.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// State.
        /// </summary>
        ElementSchema Schema { get; }

        /// <summary>
        /// Copy of children collection.
        /// </summary>
        IElement[] Children { get; }

        /// <summary>
        /// Called when this node has been removed from the graph.
        /// </summary>
        event Action<IElement> OnRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been removed from the graph.
        /// </summary>
        event Action<IElement, IElement> OnChildRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been added to the graph.
        /// </summary>
        event Action<IElement, IElement> OnChildAdded;

        /// <summary>
        /// Invoked when element is destroyed.
        /// </summary>
        event Action<IElement> OnDestroyed;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        void Load(ElementData data, ElementSchema schema, IElement[] children);

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        void FrameUpdate();

        /// <summary>
        /// Invoked one every frame after all Element's have been updated.
        /// </summary>
        void LateFrameUpdate();

        /// <summary>
        /// Adds an element as a child of this element. If the element is
        /// already a child, moves it to the end of the list.
        /// </summary>
        /// <param name="element">Element to add as a child.</param>
        void AddChild(IElement element);

        /// <summary>
        /// Removes an element as a child.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <returns></returns>
        bool RemoveChild(IElement element);

        /// <summary>
        /// Finds a single element.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns></returns>
        IElement FindOne(string query);

        /// <summary>
        /// Finds a single element.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="results">Result list.</param>
        /// <returns></returns>
        void Find(string query, IList<IElement> results);

        /// <summary>
        /// Readable string generation.
        /// </summary>
        /// <returns></returns>
        string ToTreeString();
    }
}
