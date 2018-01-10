using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Implementation that does nothing.
    /// </summary>
    public class PassthroughVoiceCommandManager : IVoiceCommandManager
    {
        /// <inheritdoc cref="IVoiceCommandManager"/>
        public bool Register(string keyword, Action<string> @delegate)
        {
            return true;
        }

        /// <inheritdoc cref="IVoiceCommandManager"/>
        public bool Unregister(params string[] keywords)
        {
            return true;
        }
    }
}