#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine.Windows.Speech;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// HoloLens implementation.
    /// </summary>
    public class VoiceCommandManager : IVoiceCommandManager
    {
        /// <summary>
        /// Threshold required between "debug" and command.
        /// </summary>
        private const int VOICE_LOCK_THRESHOLD_SECS = 3;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Tracks keywords.
        /// </summary>
        private readonly Dictionary<string, Action<string>> _actions = new Dictionary<string, Action<string>>();

        /// <summary>
        /// Recognizer!
        /// </summary>
        private KeywordRecognizer _recognizer;

        /// <summary>
        /// Debug guard.
        /// </summary>
        private KeywordRecognizer _debugGuard;

        /// <summary>
        /// The last time at which the debug guard was called.
        /// </summary>
        private DateTime _debugGuardCalled = DateTime.MinValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VoiceCommandManager(
            ApplicationConfig config,
            IMessageRouter messages)
        {
            _config = config;

            // application suspension kills voice recognizers
            messages.Subscribe(
                MessageTypes.APPLICATION_RESUME,
                _ =>
                {
                    RebuildDebugGuard();
                    RebuildRecognizer();
                });

            RebuildDebugGuard();
        }
        
        /// <inheritdoc cref="IVoiceCommandManager"/>
        public bool Register(string keyword, Action<string> callback)
        {
            if (_actions.ContainsKey(keyword))
            {
                return false;
            }

            _actions[keyword] = callback;

            RebuildRecognizer();

            return true;
        }

        /// <inheritdoc cref="IVoiceCommandManager"/>
        public bool Unregister(params string[] keywords)
        {
            var success = true;
            for (int i = 0, len = keywords.Length; i < len; i++)
            {
                success = _actions.Remove(keywords[i]) && success;
            }

            RebuildRecognizer();

            return success;
        }

        private void RebuildDebugGuard()
        {
            if (null != _debugGuard)
            {
                _debugGuard.Dispose();
            }

            _debugGuard = new KeywordRecognizer(new [] { "debug "});
            _debugGuard.OnPhraseRecognized += DebugGuard_OnPhraseRecognized;
            _debugGuard.Start();
        }

        /// <summary>
        /// Builds a new recognizer from the current keywords.
        /// </summary>
        private void RebuildRecognizer()
        {
            if (null != _recognizer)
            {
                _recognizer.Dispose();
                _recognizer = null;
            }

            var keywords = _actions.Keys.ToArray();
            if (0 == keywords.Length)
            {
                return;
            }

            _recognizer = new KeywordRecognizer(keywords);
            _recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            _recognizer.Start();
        }

        /// <summary>
        /// Called when debug is recognized.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        private void DebugGuard_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            _debugGuardCalled = DateTime.Now;
        }

        /// <summary>
        /// Called when a phrase is recognized.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            // check voice lock
            if (!_config.Debug.DisableVoiceLock)
            {
                if (DateTime.Now.Subtract(_debugGuardCalled).TotalSeconds > VOICE_LOCK_THRESHOLD_SECS)
                {
                    return;
                }
            }

            var text = args.text;

            Action<string> action;
            if (_actions.TryGetValue(text, out action))
            {
                action(text);
            }
        }
    }
}
#endif