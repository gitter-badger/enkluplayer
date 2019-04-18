using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class StandaloneMenuControls : MonoBehaviour
    {
        public event Action OnMenu;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (null != OnMenu)
                {
                    OnMenu();
                }
            }
        }
    }
}