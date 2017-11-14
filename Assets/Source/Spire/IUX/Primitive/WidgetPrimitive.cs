using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    public class WidgetPrimitive : MonoBehaviour, IPrimitive
    {
        /// <summary>
        /// Parent widget.
        /// </summary>
        public IWidget Widget { get; private set; }
       
        /// <summary>
        /// Loads using the specified widget.
        /// </summary>
        /// <param name="widget"></param>
        public void Load(IWidget widget)
        {
            Widget = widget;

            transform.SetParent(Widget.GameObject.transform, false);
            transform.gameObject.SetActive(true);
        }

        /// <summary>
        /// Unloads the primitive.
        /// </summary>
        public void Unload()
        {
            Destroy(this);
        }
    }
}
