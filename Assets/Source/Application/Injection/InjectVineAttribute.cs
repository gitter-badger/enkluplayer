using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Injects a vine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectVineAttribute : Attribute
    {
        /// <summary>
        /// Identifier of the vine.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identifier">The id of the vine.</param>
        public InjectVineAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}