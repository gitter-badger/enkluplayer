using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Acts as the glue between the webpage and Unity.
    /// </summary>
    public class WebBridge : MonoBehaviour
    {
        /// <summary>
        /// Provides a binding for events.
        /// </summary>
        private class Binding
        {
            public string MessageTypeString;
            public int MessageTypeInt;
            public Type Type;
        }

        /// <summary>
        /// Map from event string to binding.
        /// </summary>
        private readonly Dictionary<string, Binding> _messageMap = new Dictionary<string, Binding>();

        /// <summary>
        /// Routes messages.
        /// </summary>
        [Inject]
        public IMessageRouter Router { get; set; }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void Init();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void Ready();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void On(string messageTypeString);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void Off(string messageTypeString);
#endif

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Init();
#endif
        }

        /// <summary>
        /// Tells the webpage that the application is ready.
        /// </summary>
        public void BroadcastReady()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Ready();
#endif
        }

        /// <summary>
        /// Binds a message type to a Type.
        /// </summary>
        /// <typeparam name="T">The type with which to parse the event.</typeparam>
        /// <param name="messageTypeString">The message type.</param>
        /// <param name="messageTypeInt">The message type to push onto the <c>IMessageRouter</c>.</param>
        public void Bind<T>(string messageTypeString, int messageTypeInt)
        {
            if (_messageMap.ContainsKey(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} already bound.",
                    messageTypeString));
            }

            _messageMap[messageTypeString] = new Binding
            {
                MessageTypeString = messageTypeString,
                MessageTypeInt = messageTypeInt,
                Type = typeof(T)
            };

#if !UNITY_EDITOR && UNITY_WEBGL
            On(messageTypeString);
#else
            throw new Exception("WebBridge should not be used outside of WebGL target.");
#endif
        }

        /// <summary>
        /// Unbinds an event. See Bind.
        /// </summary>
        public void Unbind<T>(string messageTypeString, int messageTypeInt)
        {
            if (!_messageMap.ContainsKey(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} not bound.",
                    messageTypeString));
            }

            _messageMap.Remove(messageTypeString);

#if !UNITY_EDITOR && UNITY_WEBGL
            Off(messageTypeString);
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
            Log.Debug(this, "WebBridge Received [{0}]", message);
            
            // pull off message type
            JObject messageObj;
            try
            {
                messageObj = JObject.Parse(message);
            }
            catch (Exception exception)
            {
                Log.Error(this,
                    "Received a message that cannot be parsed : {0} : {1}.",
                    exception,
                    message);
                return;
            }

            var messageTypeObj = messageObj.SelectToken(".messageType");
            if (null == messageTypeObj)
            {
                Log.Error(
                    this,
                    "Received a message with no messageType : {0}.",
                    message);
                return;
            }

            var messageType = messageTypeObj.Value<string>();

            Binding binding;
            if (!_messageMap.TryGetValue(messageType, out binding))
            {
                Log.Fatal(
                    this,
                    "Receieved a message for which we do not have a binding : {0}.",
                    messageType);
                return;
            }

            var payloadObj = messageObj.SelectToken(".payload");
            object payload;
            try
            {
                // eek-- Newtonsoft is failing me on webgl
                payload = JsonUtility.FromJson(
                    payloadObj.ToString(),
                    binding.Type);
            }
            catch (Exception exception)
            {
                Log.Error(
                    this,
                    "Could not deserialize {0} payload to a [{1}] : {2}.",
                    messageType,
                    binding.Type,
                    exception);
                return;
            }

            Log.Debug(this,
                "Publishing a {0} event.",
                messageType);
            
            // publish
            Router.Publish(
                binding.MessageTypeInt,
                payload);
        }
    }
}