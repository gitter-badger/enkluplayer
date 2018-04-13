using System;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Acts as the glue between the webpage and Unity.
    /// </summary>
    public class WebBridge : InjectableMonoBehaviour, IBridge
    {
        /// <summary>
        /// Handler.
        /// </summary>
        private BridgeMessageHandler _handler;

        /// <summary>
        /// Routes messages.
        /// </summary>
        [Inject]
        public IMessageRouter Router { get; set; }
        
        /// <summary>
        /// Allows binding between message types and C# types.
        /// </summary>
        public MessageTypeBinder Binder { get { return _handler.Binder; } }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void init();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ready();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void message(string message);
#endif

        /// <inheritdoc cref="IBridge"/>
        public void Initialize(BridgeMessageHandler handler)
        {
            _handler = handler;

#if !UNITY_EDITOR && UNITY_WEBGL
            UnityEngine.WebGLInput.captureAllKeyboardInput = false;

            init();
#endif
        }

        /// <inheritdoc cref="IBridge"/>
        public void Uninitialize()
        {
            Binder.Clear();
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
            _handler.OnMessage(message);
#else
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }

        /// <summary>
        /// Sends a message across the bridge.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(string message)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            message(message);
#else
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }
    }
}