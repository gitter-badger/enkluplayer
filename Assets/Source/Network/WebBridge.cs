using System;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Acts as the glue between the webpage and Unity.
    /// </summary>
    public class WebBridge : InjectableMonoBehaviour, IBridge
    {
        /// <summary>
        /// Routes messages.
        /// </summary>
        [Inject]
        public IMessageRouter Router { get; set; }

        /// <summary>
        /// Parses messages.
        /// </summary>
        [Inject]
        public BridgeMessageHandler Handler { get; set; }

        /// <summary>
        /// Allows binding between message types and C# types.
        /// </summary>
        public MessageTypeBinder Binder { get { return Handler.Binder; } }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void init();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ready();
#endif
        
        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if !UNITY_EDITOR && UNITY_WEBGL
            UnityEngine.WebGLInput.captureAllKeyboardInput = false;

            init();
#endif
        }

        /// <summary>
        /// Tells the webpage that the application is ready.
        /// </summary>
        public void BroadcastReady()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ready();
#else
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }
        
        /// <summary>
        /// Called by the webpage when it's trying to tell us something.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnNetworkEvent(string message)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Handler.OnMessage(message);
#else
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }
    }
}