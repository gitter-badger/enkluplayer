using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public interface IHighlightManager
    {
        /// <summary>
        /// Retrieves the current highlighted element.
        /// </summary>
        Widget Highlighted { get; }
    }
}
