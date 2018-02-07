using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// References a vine.
    /// </summary>
    [Serializable]
    public class VineReference
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Source.
        /// </summary>
        public TextAsset Source;

        /// <summary>
        /// Retrieves text.
        /// </summary>
        public string Text
        {
            get { return null == Source ? "" : Source.text; }
        }
    }
}