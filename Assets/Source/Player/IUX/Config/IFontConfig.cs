using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Defines an interface for working with fonts.
    /// </summary>
    public interface IFontConfig
    {
        /// <summary>
        /// Current font profile.
        /// </summary>
        FontProfile Current { get; set; }

        /// <summary>
        /// Retrieves a font.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="font">The requested font.</param>
        /// <returns></returns>
        bool Font(string fontName, out Font font);
    }
}