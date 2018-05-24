using System;

namespace CreateAR.SpirePlayer.Mobile
{
    public class MobileAppSearchUIView : MonoBehaviourUIElement
    {
        
        
        public event Action<string> OnQueryUpdated;

        public event Action<string> OnAppSelected;
    }
}