namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface that describes how to match a query against a set of tags.
    /// </summary>
    public interface IQueryResolver
    {
        /// <summary>
        /// Returns true iff the query matches the set of tags passed in.
        /// </summary>
        /// <param name="query">A string query.</param>
        /// <param name="tags">A list of strings.</param>
        /// <returns></returns>
        bool Resolve(string query, ref string[] tags);
    }
}