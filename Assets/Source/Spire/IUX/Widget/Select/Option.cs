﻿using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic element for Options.
    /// </summary>
    public class Option : Element
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Option(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }

    /// <summary>
    /// Basic element for OptionGroups.
    /// </summary>
    public class OptionGroup : Element
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionGroup(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}