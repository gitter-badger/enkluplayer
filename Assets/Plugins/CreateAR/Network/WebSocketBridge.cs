using System.Runtime.InteropServices;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.Spire
{
    public class WebSocketBridge : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void Init();

        [DllImport("__Internal")]
        public static extern void On(string messageType);

        [DllImport("__Internal")]
        public static extern void Off(string messageType);

        private void Awake()
        {
            Init();

            On("assetcreation");
        }

        public void OnNetworkEvent(string message)
        {
            Log.Debug("Received [{0}]", message);
        }
    }
}
