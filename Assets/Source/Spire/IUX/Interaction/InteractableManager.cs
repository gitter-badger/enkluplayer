using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic implementation of <c>IInteractableManager</c>.
    /// </summary>
    public class InteractableManager : IInteractableManager
    {
        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private readonly List<IInteractable> _all = new List<IInteractable>();

        /// <inheritdoc cref="IInteractableManager"/>
        public ReadOnlyCollection<IInteractable> All { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InteractableManager()
        {
            All = new ReadOnlyCollection<IInteractable>(_all);
        }

        /// <inheritdoc cref="IInteractableManager"/>
        public void Add(IInteractable interactable)
        {
            _all.Remove(interactable);
            _all.Add(interactable);
        }

        /// <inheritdoc cref="IInteractableManager"/>
        public void Remove(IInteractable interactable)
        {
            _all.Remove(interactable);
        }
    }
}