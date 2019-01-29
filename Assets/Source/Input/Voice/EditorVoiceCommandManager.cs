using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Editor implementation of voice commands.
    /// </summary>
    public class EditorVoiceCommandManager : IVoiceCommandManager
    {
        /// <summary>
        /// Callbacks.
        /// </summary>
        public readonly Dictionary<string, Action<string>> Callbacks = new Dictionary<string, Action<string>>();
        public readonly Dictionary<string, Action<string>> AdminCallbacks = new Dictionary<string, Action<string>>();

        /// <summary>
        /// Keep a reference so the EditorWindow can see it.
        /// </summary>
        public static EditorVoiceCommandManager Instance;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorVoiceCommandManager()
        {
            Instance = this;
        }

        /// <inheritdoc />
        public bool Register(string keyword, Action<string> @delegate)
        {
            Callbacks[keyword] = @delegate;

            return true;
        }

        /// <inheritdoc />
        public bool RegisterAdmin(string keyword, Action<string> @delegate)
        {
            Callbacks.Remove(keyword);
            AdminCallbacks[keyword] = @delegate;

            return true;
        }

        /// <inheritdoc />
        public bool Unregister(params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                Callbacks.Remove(keyword);
                AdminCallbacks.Remove(keyword);
            }

            return true;
        }

        /// <summary>
        /// Calls a specific command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public bool Call(string command)
        {
            Action<string> callback;
            if (!Callbacks.TryGetValue(command, out callback))
            {
                if (!AdminCallbacks.TryGetValue(command, out callback))
                {
                    return false;
                }
            }

            callback(command);
            return true;
        }
    }
}