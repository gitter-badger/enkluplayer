using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages icons.
    /// </summary>
    [Serializable]
    public class IconConfig
    {
        /// <summary>
        /// Icons.
        /// </summary>
        public Sprite[] Icons;

        /// <summary>
        /// Retrieves an icon by name.
        /// </summary>
        /// <param name="name">Name of the icon.</param>
        /// <returns></returns>
        public Sprite Icon(string name)
        {
            if (null == Icons)
            {
                return null;
            }

            for (int i = 0, len = Icons.Length; i < len; i++)
            {
                var icon = Icons[i];
                if (null != icon && icon.name == name)
                {
                    return icon;
                }
            }

            return null;
        }
    }
}