using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    public class WidgetPrimitive : MonoBehaviour, IPrimitive
    {
        /// <summary>
        /// Transform accessor.
        /// </summary>
        public Transform Transform { get { return transform; } }

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="schema"></param>
        public virtual void Load(ElementSchema schema)
        {
            // empty
        }
    }
}
