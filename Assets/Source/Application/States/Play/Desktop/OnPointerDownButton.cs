using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Button subclass that dispatches OnClick events on pointer down.
    /// </summary>
    public class OnPointerDownButton : Button
    {
        /// <summary>
        /// Called when clicked.
        /// </summary>
        public event Action OnClicked;

        /// <inheritdoc />
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (null != OnClicked)
            {
                OnClicked();
            }
        }
    }
}