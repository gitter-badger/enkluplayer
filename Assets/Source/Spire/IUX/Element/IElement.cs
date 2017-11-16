using System;

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
        /// Called when a child, at any depth, has been removed from the graph.
        /// </summary>
        event Action<IElement, IElement> OnChildRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been added to the graph.
        /// </summary>
        event Action<IElement, IElement> OnChildAdded;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        void Load(ElementData data, ElementSchema schema, IElement[] children);
    }
}
