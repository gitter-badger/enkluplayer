﻿using System.Collections.ObjectModel;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages interactable objects.
    /// </summary>
    public interface IInteractableManager
    {
        /// <summary>
        /// Collection of all <c>IInteractable</c> instances.
        /// </summary>
        ReadOnlyCollection<IInteractable> All { get; }

        /// <summary>
        /// Adds an interactable.
        /// </summary>
        /// <param name="interactable">The element to add.</param>
        void Add(IInteractable interactable);

        /// <summary>
        /// Removes an interactable.
        /// </summary>
        /// <param name="interactable">The element to remove.</param>
        void Remove(IInteractable interactable);
    }
}