using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// View for simple messages.
    /// </summary>
    public class MobileMessageUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Unity UI.
        /// </summary>
        [SerializeField]
        private Text _title;
        [SerializeField]
        private Text _description;
        [SerializeField]
        private Text _action;
        
        /// <summary>
        /// Gets/sets the title.
        /// </summary>
        public string Title
        {
            get { return _title.text; }
            set { _title.text = value; }
        }
        
        /// <summary>
        /// Gets/sets the description.
        /// </summary>
        public string Description
        {
            get { return _description.text; }
            set { _description.text = value; }
        }

        /// <summary>
        /// Gets/sets the button label.
        /// </summary>
        public string Action
        {
            get { return _action.text; }
            set { _action.text = value; }
        }

        /// <summary>
        /// Called when button is pressed.
        /// </summary>
        public event Action OnOk;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void OkayClicked()
        {
            if (null != OnOk)
            {
                OnOk();
            }
        }
    }
}