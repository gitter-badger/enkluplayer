using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for interacting with voice commands.
    /// </summary>
    public interface IVoiceCommandManager
    {
        /// <summary>
        /// Registers a keyword for detection.
        /// </summary>
        /// <param name="keyword">The keyword to look for.</param>
        /// <param name="delegate">The delegate that will be called.</param>
        /// <returns></returns>
        bool Register(string keyword, Action<string> @delegate);

        /// <summary>
        /// Registers a keyword for detection.
        /// Admin keywords are guarded by the "admin" phrase.
        /// </summary>
        /// <param name="keyword">The keyword to look for.</param>
        /// <param name="delegate">The delegate that will be called.</param>
        /// <returns></returns>
        bool RegisterAdmin(string keyword, Action<string> @delegate);

        /// <summary>
        /// Unregisters a keyword.
        /// </summary>
        /// <param name="keywords">The keyword to unregister.</param>
        /// <returns></returns>
        bool Unregister(params string[] keywords);
    }
}