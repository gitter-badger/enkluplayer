using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state within the editor.
    /// </summary>
    public class EditorApplicationState : IApplicationState
    {
        /// <summary>
        /// Path to Value lookup.
        /// </summary>
        public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

        /// <inheritdoc cref="IApplicationState"/>
        public bool Get(string path, out string value)
        {
            object val;
            if (!Values.TryGetValue(path, out val))
            {
                value = string.Empty;
                return false;
            }

            value = (string) val;
            return true;
        }
    }
}