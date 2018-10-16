﻿#if NETFX_CORE
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
        private struct PhraseConfig
        {
            public Action<string> Callback;
            public bool AdminPhrase;
        }
        
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
        private readonly Dictionary<string, PhraseConfig> _actions = new Dictionary<string, PhraseConfig>();

        /// <summary>
        /// Recognizer!
        /// </summary>
        private KeywordRecognizer _recognizer;

        /// <summary>
        /// Debug guard.
        /// </summary>
        private KeywordRecognizer _debugGuard;

        /// <summary>
        /// Admin guard.
        /// </summary>
        private KeywordRecognizer _adminGuard;

        /// <summary>
        /// The last time at which the debug guard was called.
        /// </summary>
        private DateTime _debugGuardCalled = DateTime.MinValue;

        /// <summary>
        /// The last time at which the admin guard was called.
        /// </summary>
        private DateTime _adminGuardCalled = DateTime.MinValue;

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
                    RebuildAdminGuard();
                    RebuildRecognizer();
                });

            RebuildDebugGuard();
            RebuildAdminGuard();
        }
        
        /// <inheritdoc cref="IVoiceCommandManager"/>
        public bool Register(string keyword, Action<string> callback, bool adminCommand = false)
        {
            if (_actions.ContainsKey(keyword))
            {
                return false;
            }

            _actions[keyword] = new PhraseConfig()
            {
                Callback = callback,
                AdminPhrase = adminCommand
            };

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

        /// <summary>
        /// Rebuilds the debug guard.
        /// </summary>
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

        private void RebuildAdminGuard()
        {
            if (null != _adminGuard)
            {
                _adminGuard.Dispose();
            }

            _adminGuard = new KeywordRecognizer(new [] { "admin " });
            _adminGuard.OnPhraseRecognized += AdminGuard_OnPhraseRecognized;
            _adminGuard.Start();
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
        /// Called when admin is recognized.
        /// </summary>
        /// <param name="args"></param>
        private void AdminGuard_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            if (!_config.Debug.DisableVoiceLock)
            {
                if (DateTime.Now.Subtract(_debugGuardCalled).TotalSeconds > VOICE_LOCK_THRESHOLD_SECS)
                {
                    return;
                }
            }
            
            _adminGuardCalled = DateTime.Now;
        }

        /// <summary>
        /// Called when a phrase is recognized.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            var text = args.text;

            PhraseConfig phraseConfig;
            if (_actions.TryGetValue(text, out phraseConfig))
            {
                // check extra admin lock
                if (phraseConfig.AdminPhrase && !_config.Debug.DisableAdminLock)
                {
                    if (DateTime.Now.Subtract(_adminGuardCalled).TotalSeconds < VOICE_LOCK_THRESHOLD_SECS) {
                        phraseConfig.Callback(text);
                    }
                    return;
                }

                // check voice lock
                if (!_config.Debug.DisableVoiceLock)
                {
                    if (DateTime.Now.Subtract(_debugGuardCalled).TotalSeconds > VOICE_LOCK_THRESHOLD_SECS)
                    {
                        return;
                    }
                }

                phraseConfig.Callback(text);
            }
        }
    }
}
#endif