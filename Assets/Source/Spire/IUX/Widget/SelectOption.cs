using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic element for Options.
    /// </summary>
    public class SelectOption : Element
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectOption(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}