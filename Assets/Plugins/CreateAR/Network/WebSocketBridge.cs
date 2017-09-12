using System.Runtime.InteropServices;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.Spire
{
    public class AssetModel
    {
        public string id;
        public string owner;
        public int version;
        public string name;
    }

    public class Message
    {
        public string messageType;
        public object payload;
    }

    public class RequestPreviewEvent
    {
        public AssetModel Asset;
    }

    public class Handler
    {
        

        public void Handle(RequestPreviewEvent message)
        {
            
        }
    }

    public class WebSocketBridge : MonoBehaviour
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern void Init();

        [DllImport("__Internal")]
        public static extern void On(string messageType);

        [DllImport("__Internal")]
        public static extern void Off(string messageType);

        private void Awake()
        {
            Init();

            On("preview");
        }

        public void OnNetworkEvent(string message)
        {
            Log.Debug("Received [{0}]", message);

            
        }
#endif
    }
}