using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Allows debug / admin voice commands to be registered from scripting.
    /// </summary>
    [JsInterface("voice")]
    public class VoiceJsInterface
    {
        /// <summary>
        /// Helper for managing scripting callbacks.
        /// </summary>
        private struct JsCallback
        {
            /// <summary>
            /// The function to invoke.
            /// </summary>
            public JsFunc Callback;
            
            /// <summary>
            /// The context to invoke it with.
            /// </summary>
            public JsValue This;
        }
        
        /// <summary>
        /// The backing IVoiceCommandManager.
        /// </summary>
        private IVoiceCommandManager _voice;
        
        /// <summary>
        /// Cache of bound commands/callbacks.
        /// </summary>
        private Dictionary<string, JsCallback> _callbacks = new Dictionary<string, JsCallback>(); 

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="voice"></param>
        public VoiceJsInterface(IVoiceCommandManager voice)
        {
            _voice = voice;
        }
        
        /// <summary>
        /// Registers a voice command to trigger a callback.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="command"></param>
        /// <param name="callback"></param>
        public void register(Engine engine, string command, JsFunc callback)
        {
            if (_voice.Register(command, Voice_OnRecognized))
            {
                _callbacks[command] = new JsCallback
                {
                    This = JsValue.FromObject(engine, this),
                    Callback = callback
                };
            }
            else
            {
                Log.Error(this, "Voice command already registered ({0})", command);
            }
        }

        /// <summary>
        /// Registers an admin voice command to trigger a callback.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="command"></param>
        /// <param name="callback"></param>
        public void registerAdmin(Engine engine, string command, JsFunc callback)
        {
            if (_voice.RegisterAdmin(command, Voice_OnRecognized))
            {
                _callbacks[command] = new JsCallback
                {
                    This = JsValue.FromObject(engine, this),
                    Callback = callback
                };
            }
            else
            {
                Log.Error(this, "Voice command already registered ({0})", command);
            }
        }

        /// <summary>
        /// Unregisters a voice command.
        /// </summary>
        /// <param name="command"></param>
        public void unregister(string command)
        {
            if (_voice.Unregister(command))
            {
                _callbacks.Remove(command);
            }
            else
            {
                Log.Error(this, "Voice command already registered ({0})", command);
            }
        }

        /// <summary>
        /// Invoked when a registered command is recognized.
        /// </summary>
        /// <param name="command"></param>
        private void Voice_OnRecognized(string command)
        {
            JsCallback jsCallback;
            if (!_callbacks.TryGetValue(command, out jsCallback))
            {
                Log.Error(this, "No callback mapped for command ({0})", command);
                return;
            }

            try
            {
                jsCallback.Callback(jsCallback.This, new JsValue[] { command });
            }
            catch (Exception e)
            {
                Log.Error(this, "Error invoking scripting callback ({0})", e);
            }
            
        }
    }
}