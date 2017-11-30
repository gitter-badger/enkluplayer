﻿namespace CreateAR.SpirePlayer
{
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
    }
}
