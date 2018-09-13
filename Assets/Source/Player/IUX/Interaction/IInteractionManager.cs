using System;
using System.Collections.ObjectModel;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Interface for interaction management.
    /// </summary>
    public interface IInteractionManager
    {
        /// <summary>
        /// Retrieves the current highlighted element.
        /// </summary>
        IInteractable Highlighted { get; }

        /// <summary>
        /// True if interaction is locked to only the highlighed widget.
        /// </summary>
        bool IsOnRails { get; }

        /// <summary>
        /// Retrieves all interactables.
        /// </summary>
        ReadOnlyCollection<IInteractable> All { get; }

        /// <summary>
        /// Retrieves only visible interactables.
        /// </summary>
        ReadOnlyCollection<IInteractable> Visible { get; }

        /// <summary>
        /// Called when an interactable has been unmanaged.
        /// </summary>
        event Action<IInteractable> OnRemoved;

        /// <summary>
        /// Called when an interactable has been managed.
        /// </summary>
        event Action<IInteractable> OnAdded;

        /// <summary>
        /// Adds an interactable to the manager.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        void Add(IInteractable interactable);

        /// <summary>
        /// Removes an interactable from the manager.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        void Remove(IInteractable interactable);

        /// <summary>
        /// Adds an object to highlight queue. The element with the highest
        /// HighlightPriority will be highlighted.
        /// 
        /// The Highlighted property is updated every frame, not synchronously.
        /// </summary>
        /// <param name="interactable">The element to add.</param>
        void Highlight(IInteractable interactable);

        /// <summary>
        /// Unhighlights an element.
        /// 
        /// The Highlighted property is updated every frame, not synchronously.
        /// </summary>
        /// <param name="interactable">The element to remove.</param>
        void Unhighlight(IInteractable interactable);
    }
}