using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire
{
    /// <summary>
    /// Acts as the glue between the webpage and Unity.
    /// </summary>
    public class WebBridge : MonoBehaviour, IBridge
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
        public IMessageParser Parser { get; set; }

        public DataBinder Binder { get; private set; }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void init();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ready();
#endif

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            Binder = new DataBinder();
        }

        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        public void Init()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
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
            Log.Debug(this, "Received [{0}]", message);
            
            // parse
            string messageTypeString;
            string payloadString;
            if (!Parser.ParseMessage(
                message,
                out messageTypeString,
                out payloadString))
            {
                Log.Warning(
                    this,
                    "Received a message that cannot be parsed : {0}.", message);
                return;
            }

            var binding = Binder.ByMessageType(messageTypeString);
            if (null == binding)
            {
                Log.Fatal(
                    this,
                    "Received a message for which we do not have a binding : {0}.",
                    messageTypeString);
                return;
            }

            object payload;
            if (binding.Type == typeof(Void))
            {
                payload = Void.Instance;
            }
            else
            {
                try
                {
                    // eek-- Newtonsoft is failing me on webgl
                    payload = JsonUtility.FromJson(
                        payloadString,
                        binding.Type);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        this,
                        "Could not deserialize {0} payload to a [{1}] : {2}.",
                        messageTypeString,
                        binding.Type,
                        exception);
                    return;
                }
            }

            Log.Debug(this,
                "Publishing a {0} event.",
                messageTypeString);
            
            // publish
            Router.Publish(
                binding.MessageTypeInt,
                payload);

#if UNITY_EDITOR || !UNITY_WEBGL
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }
    }
}