using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// A set of font information.
    /// </summary>
    [Serializable]
    public class FontProfile
    {
        /// <summary>
        /// Set of fonts.
        /// </summary>
        public FontEntry[] Fonts;

        /// <summary>
        /// Retrieves a font.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="font">The returned font.</param>
        /// <returns></returns>
        public bool Font(string fontName, out Font font)
        {
            for (int i = 0, len = Fonts.Length; i < len; i++)
            {
                if (Fonts[i].Name == fontName)
                {
                    font = Fonts[i].Font;
                    return true;
                }

            }

            font = null;
            return false;
        }
    }
}