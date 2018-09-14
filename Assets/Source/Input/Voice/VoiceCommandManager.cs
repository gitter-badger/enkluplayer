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
        /// <summary>
        /// Tracks keywords.
        /// </summary>
        private readonly Dictionary<string, Action<string>> _actions = new Dictionary<string, Action<string>>();

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Recognizer!
        /// </summary>
        private KeywordRecognizer _recognizer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VoiceCommandManager(IMessageRouter messages)
        {
            _messages = messages;

            _messages.Subscribe(
                MessageTypes.APPLICATION_RESUME,
                _ => RebuildRecognizer());
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

        /// <summary>
        /// Builds a new recognizer from the current keywords.
        /// </summary>
        private void RebuildRecognizer()
        {
            if (null != _recognizer)
            {
                _recognizer.Dispose();
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
        /// Called when a phrase is recognized.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
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