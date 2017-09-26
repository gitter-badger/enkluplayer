using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class PreviewOptionsBehaviour : InjectableMonoBehaviour
    {
        [Inject]
        public IMessageRouter Messages { get; set; }

        private void OnGUI()
        {
            if (Button("Load Model"))
            {
               Messages.Publish(
                   MessageTypes.PREVIEW,
                   new PreviewEvent());
            }
        }

        private bool Button(string label)
        {
            return GUILayout.Button(
                label,
                GUILayout.Height(40),
                GUILayout.ExpandWidth(false));
        }
    }
}
