using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Vine
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

        /// <summary>
        /// Called when the reference has been updated.
        /// </summary>
        public event Action OnUpdated;

        /// <summary>
        /// Calls OnUpdated method.
        /// </summary>
        public void Updated()
        {
            if (null != OnUpdated)
            {
                OnUpdated();
            }
        }
    }
}