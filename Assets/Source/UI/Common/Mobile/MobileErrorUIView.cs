﻿using System;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls the view for an AR error.
    /// </summary>
    public class MobileErrorUIView : MonoBehaviourUIElement, ICommonErrorView
    {
        /// <inheritdoc />
        public string Message
        {
            get { return Description.text; }
            set { Description.text = value; }    
        }

        /// <inheritdoc />
        public string Action
        {
            get { return ButtonText.text; }
            set { ButtonText.text = value; }
        }

        /// <inheritdoc />
        public event Action OnOk;

        /// <summary>
        /// Unity UI object.
        /// </summary>
        public Text Description;

        /// <summary>
        /// Unity UI object.
        /// </summary>
        public Text ButtonText;

        /// <summary>
        /// Action button.
        /// </summary>
        public Button Button;

        /// <inheritdoc />
        public void DisableAction()
        {
            Button.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called by the UI when the OK button is touched.
        /// </summary>
        public void OnOkayClicked()
        {
            if (null != OnOk)
            {
                OnOk();
            }   
        }
    }
}