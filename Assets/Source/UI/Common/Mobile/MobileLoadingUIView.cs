using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Loading view.
    /// </summary>
    public class MobileLoadingUIView : MonoBehaviourUIElement, ICommonLoadingView
    {
        /// <summary>
        /// Text.
        /// </summary>
        public Text Text;

        /// <summary>
        /// Status.
        /// </summary>
        public string Status
        {
            get { return Text.text; }
            set { Text.text = value; }
        }
    }
}