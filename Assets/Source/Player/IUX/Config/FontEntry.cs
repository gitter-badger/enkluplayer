using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Links a name and a font.
    /// </summary>
    [Serializable]
    public class FontEntry
    {
        /// <summary>
        /// Name associated with this font.
        /// </summary>
        public string Name;

        /// <summary>
        /// The font.
        /// </summary>
        public Font Font;
    }
}