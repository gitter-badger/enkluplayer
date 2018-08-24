using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for <c>SpireScript</c> management.
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        /// Finds the first instance of a <c>SpireScript</c> with the matching
        /// <c>ScriptData</c> id.
        /// </summary>
        /// <param name="id">Id of the <c>ScriptData</c>.</param>
        /// <returns></returns>
        SpireScript FindOne(string id);

        /// <summary>
        /// Finds all currently executing scripts by script id.
        /// </summary>
        /// <param name="id">Id of the <c>ScriptData</c>.</param>
        /// <param name="scripts">List to add found scripts to.</param>
        void FindAll(string id, List<SpireScript> scripts);

        /// <summary>
        /// Finds first currently executing script with matching tags.
        /// </summary>
        /// <param name="query">Query given by <c>IQueryResolver</c> implementation.</param>
        SpireScript FindOneTagged(string query);

        /// <summary>
        /// Finds all currently executing scripts with matching tags.
        /// </summary>
        /// <param name="query">Query given by <c>IQueryResolver</c> implementation.</param>
        /// <param name="scripts">List to add found scripts to.</param>
        void FindAllTagged(string query, List<SpireScript> scripts);

        /// <summary>
        /// Requests a new <c>SpireScript</c> instance.
        /// </summary>
        /// <param name="scriptId">Unique id for the script.</param>
        /// <param name="tags">Associated meta. These are kept with the instance
        /// so that it may be cleaned up later.</param>
        /// <returns></returns>
        SpireScript Create(string scriptId, params string[] tags);

        /// <summary>
        /// Passes a message to set of scripts.
        /// </summary>
        /// <param name="query">Query string.</param>
        /// <param name="name">Name of the message.</param>
        /// <param name="parameters">The parameters.</param>
        void Send(string query, string name, params object[] parameters);

        /// <summary>
        /// Releases an instance of <c>SpireScript</c>.
        /// </summary>
        /// <param name="script">The script to release.</param>
        void Release(SpireScript script);

        /// <summary>
        /// Releases all <c>SpireScript</c> instances that have no tags other
        /// than these.
        /// </summary>
        /// <param name="tags">The tags.</param>
        void ReleaseAll(params string[] tags);
    }
}