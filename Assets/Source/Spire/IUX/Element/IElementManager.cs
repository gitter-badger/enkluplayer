﻿using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public interface IElementManager
    {
        /// <summary>
        /// Adds an element.
        /// </summary>
        /// <param name="element">The element to add.</param>
        void Add(IElement element);
    }
}
