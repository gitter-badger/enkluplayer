namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interim interface for polling application state.
    /// </summary>
    public interface IApplicationState
    {
        /// <summary>
        /// Retrieves a piece of state.
        /// </summary>
        /// <param name="path">The path inside the state.</param>
        /// <param name="value">The string value.</param>
        /// <returns>True iff the value was found.</returns>
        bool Get(string path, out string value);
    }
}