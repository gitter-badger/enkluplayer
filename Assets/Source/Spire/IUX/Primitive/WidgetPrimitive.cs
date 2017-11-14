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
        public virtual void Load(IWidget widget)
        {
            Widget = widget;

            transform.SetParent(Widget.GameObject.transform, false);
            transform.gameObject.SetActive(true);
        }

        /// <summary>
        /// Unloads the primitive.
        /// </summary>
        public virtual void Unload()
        {
            Destroy(this);
        }

        /// <summary>
        /// Toggles Widget Visibility
        /// </summary>
        [ContextMenu("Show")]
        public void ToggleVisibility()
        {
            Widget.LocalVisible = !Widget.LocalVisible;
        }
    }
}
