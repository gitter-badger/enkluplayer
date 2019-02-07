#if NETFX_CORE || (!UNITY_EDITOR && UNITY_WSA)
using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine.Windows.Speech;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// HoloLens implementation.
    /// </summary>
    public class VoiceCommandManager : IVoiceCommandManager
    {
        private struct KeywordConfig
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
        private readonly Dictionary<string, KeywordConfig> _actions = new Dictionary<string, KeywordConfig>();

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
        public bool Register(string keyword, Action<string> callback)
        {
            return RegisterKeyword(keyword, callback, false);
        }

        public bool RegisterAdmin(string keyword, Action<string> callback)
        {
            return RegisterKeyword(keyword, callback, true);
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
        /// Registers a keyword with the voice recognizer.
        /// </summary>
        /// <param name="keyword">Keyword to bind to.</param>
        /// <param name="callback">Callback to invoke.</param>
        /// <param name="adminCommand">If this keyword should be guarded by "admin".</param>
        /// <returns></returns>
        private bool RegisterKeyword(string keyword, Action<string> callback, bool adminCommand)
        {
            if (_actions.ContainsKey(keyword)) {
                return false;
            }

            _actions[keyword] = new KeywordConfig {
                Callback = callback,
                AdminPhrase = adminCommand
            };

            RebuildRecognizer();

            return true;
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

            try
            {
                _debugGuard = new KeywordRecognizer(new[] {"debug "});
                _debugGuard.OnPhraseRecognized += DebugGuard_OnPhraseRecognized;
                _debugGuard.Start();
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
        }

        /// <summary>
        /// Rebuilds the admin guard.
        /// </summary>
        private void RebuildAdminGuard()
        {
            if (null != _adminGuard)
            {
                _adminGuard.Dispose();
            }

            try
            {
                _adminGuard = new KeywordRecognizer(new[] {"admin "});
                _adminGuard.OnPhraseRecognized += AdminGuard_OnPhraseRecognized;
                _adminGuard.Start();
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
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

            try
            {
                _recognizer = new KeywordRecognizer(keywords);
                _recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
                _recognizer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
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

            KeywordConfig keywordConfig;
            if (_actions.TryGetValue(text, out keywordConfig))
            {
                // check extra admin lock
                if (keywordConfig.AdminPhrase && !_config.Debug.DisableAdminLock)
                {
                    if (DateTime.Now.Subtract(_adminGuardCalled).TotalSeconds < VOICE_LOCK_THRESHOLD_SECS) {
                        keywordConfig.Callback(text);
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

                keywordConfig.Callback(text);
            }
        }
    }
}
#endif