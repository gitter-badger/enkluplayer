﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Acts as the glue between the webpage and Unity.
    /// </summary>
    public class WebBridge : MonoBehaviour, IBridge
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

        /// <summary>
        /// Parses messages.
        /// </summary>
        [Inject]
        public IMessageParser Parser { get; set; }

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void init();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ready();
#endif
        
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

#if UNITY_EDITOR || UNITY_WEBGL
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

#if UNITY_EDITOR || UNITY_WEBGL
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

            Binding binding;
            if (!_messageMap.TryGetValue(messageTypeString, out binding))
            {
                Log.Fatal(
                    this,
                    "Receieved a message for which we do not have a binding : {0}.",
                    messageTypeString);
                return;
            }

            object payload;
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