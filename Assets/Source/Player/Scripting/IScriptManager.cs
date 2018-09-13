using System.Collections.Generic;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for <c>EnkluScript</c> management.
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        /// Finds the first instance of a <c>EnkluScript</c> with the matching
        /// <c>ScriptData</c> id.
        /// </summary>
        /// <param name="id">Id of the <c>ScriptData</c>.</param>
        /// <returns></returns>
        EnkluScript FindOne(string id);

        /// <summary>
        /// Finds all currently executing scripts by script id.
        /// </summary>
        /// <param name="id">Id of the <c>ScriptData</c>.</param>
        /// <param name="scripts">List to add found scripts to.</param>
        void FindAll(string id, List<EnkluScript> scripts);

        /// <summary>
        /// Finds first currently executing script with matching tags.
        /// </summary>
        /// <param name="query">Query given by <c>IQueryResolver</c> implementation.</param>
        EnkluScript FindOneTagged(string query);

        /// <summary>
        /// Finds all currently executing scripts with matching tags.
        /// </summary>
        /// <param name="query">Query given by <c>IQueryResolver</c> implementation.</param>
        /// <param name="scripts">List to add found scripts to.</param>
        void FindAllTagged(string query, List<EnkluScript> scripts);

        /// <summary>
        /// Requests a new <c>EnkluScript</c> instance.
        /// </summary>
        /// <param name="scriptId">Unique id for the script.</param>
        /// <param name="tags">Associated meta. These are kept with the instance
        /// so that it may be cleaned up later.</param>
        /// <returns></returns>
        EnkluScript Create(string scriptId, params string[] tags);

        /// <summary>
        /// Passes a message to set of scripts.
        /// </summary>
        /// <param name="query">Record query (not ScriptData).</param>
        /// <param name="name">Name of the message.</param>
        /// <param name="parameters">The parameters.</param>
        void Send(string query, string name, params object[] parameters);

        /// <summary>
        /// Releases an instance of <c>EnkluScript</c>.
        /// </summary>
        /// <param name="script">The script to release.</param>
        void Release(EnkluScript script);

        /// <summary>
        /// Releases all <c>EnkluScript</c> instances that have no tags other
        /// than these.
        /// </summary>
        /// <param name="tags">The tags.</param>
        void ReleaseAll(params string[] tags);
    }
}