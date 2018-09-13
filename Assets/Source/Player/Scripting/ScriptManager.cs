using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates, queries, and destroys <c>EnkluScript</c> instances.
    /// </summary>
    public class ScriptManager : IScriptManager
    {
        /// <summary>
        /// For internal bookkeeping.
        /// </summary>
        private class ScriptRecord
        {
            /// <summary>
            /// Script.
            /// </summary>
            public readonly EnkluScript Script;

            /// <summary>
            /// Set of unique tags. These are used to cleanup references.
            /// </summary>
            public string[] Tags = new string[0];

            /// <summary>
            /// Creates a new record.
            /// </summary>
            /// <param name="script">The script instance.</param>
            /// <param name="tags">Tags associated with creation.</param>
            public ScriptRecord(EnkluScript script, params string[] tags)
            {
                Script = script;

                Tag(tags);
            }

            /// <summary>
            /// Adds a set of tags.
            /// </summary>
            /// <param name="tags">The tags to add.</param>
            public void Tag(params string[] tags)
            {
                for (int i = 0, len = tags.Length; i < len; i++)
                {
                    Tags = Tags.Add(tags[i]);
                }
            }

            /// <summary>
            /// Removes a set of tags.
            /// </summary>
            /// <param name="tags">The tags to remove.</param>
            public void Untag(params string[] tags)
            {
                for (int i = 0, len = tags.Length; i < len; i++)
                {
                    Tags = Tags.Remove(tags[i]);
                }
            }
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAppDataManager _appData;
        private readonly IScriptParser _parser;
        private readonly IScriptLoader _loader;
        private readonly IQueryResolver _resolver;

        /// <summary>
        /// Tracks all scripts.
        /// </summary>
        private readonly List<ScriptRecord> _records = new List<ScriptRecord>();

        /// <summary>
        /// Scratch list of records that we populate before we dispatch script Update.
        /// </summary>
        private readonly List<ScriptRecord> _scratchList = new List<ScriptRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appData">App data.</param>
        /// <param name="parser">For parsing scripts, asynchronously.</param>
        /// <param name="loader">For loading scripts, asynchronously.</param>
        /// <param name="resolver">Resolves queries.</param>
        public ScriptManager(
            IAppDataManager appData,
            IScriptParser parser,
            IScriptLoader loader,
            IQueryResolver resolver)
        {
            _appData = appData;
            _parser = parser;
            _loader = loader;
            _resolver = resolver;

            _appData.OnUpdated += AppData_OnUpdated;
            _appData.OnRemoved += AppData_OnRemoved;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public EnkluScript FindOne(string id)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Script.Data.Id == id)
                {
                    return record.Script;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void FindAll(string id, List<EnkluScript> scripts)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Script.Data.Id == id)
                {
                    scripts.Add(record.Script);
                }
            }
        }

        /// <inheritdoc cref="IScriptManager"/>
        public EnkluScript FindOneTagged(string query)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                var tags = record.Script.Data.Tags;
                if (_resolver.Resolve(query, ref tags))
                {
                    return record.Script;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void FindAllTagged(string query, List<EnkluScript> scripts)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                var tags = record.Script.Data.Tags;
                if (_resolver.Resolve(query, ref tags))
                {
                    scripts.Add(record.Script);
                }
            }
        }

        /// <inheritdoc cref="IScriptManager"/>
        public EnkluScript Create(string scriptId, params string[] tags)
        {
            var data = _appData.Get<ScriptData>(scriptId);
            if (null == data)
            {
                Log.Warning(this,
                    "Could not find ScriptData by id {0}.",
                    scriptId);
                return null;
            }
            
            var script = new EnkluScript(_parser, _loader, data);

            _records.Add(new ScriptRecord(script, tags));

            return script;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void Send(string query, string name, params object[] parameters)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (_resolver.Resolve(query, ref record.Tags))
                {
                    record.Script.Send(name, parameters);
                }
            }
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void Release(EnkluScript script)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Script == script)
                {
                    script.Release();

                    _records.RemoveAt(i);
                    break;
                }
            }
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void ReleaseAll(params string[] tags)
        {
            for (var i = _records.Count - 1; i >= 0; i--)
            {
                var record = _records[i];
                record.Untag(tags);

                if (0 == record.Tags.Length)
                {
                    record.Script.Release();

                    _records.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Called when a piece of data has been removed.
        /// </summary>
        /// <param name="staticData">Static data that was removed.</param>
        private void AppData_OnRemoved(StaticData staticData)
        {
            var data = staticData as ScriptData;
            if (null == data)
            {
                return;
            }

            var id = data.Id;
            for (var i = _records.Count - 1; i >= 0; i--)
            {
                var record = _records[i];
                if (record.Script.Data.Id == id)
                {
                    record.Script.Release();

                    _records.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Called when a piece of data has been updated.
        /// </summary>
        /// <param name="staticData">Static data that was updated.</param>
        private void AppData_OnUpdated(StaticData staticData)
        {
            var data = staticData as ScriptData;
            if (null == data)
            {
                return;
            }
            
            _scratchList.Clear();

            // accumulate all script updates before we call any of them, as an
            // update can cause many script reloads
            var id = data.Id;
            for (var i = 0; i < _records.Count; i++)
            {
                var record = _records[i];
                if (record.Script.Data.Id == id)
                {
                    _scratchList.Add(record);
                }
            }

            for (int i = 0, len = _scratchList.Count; i < len; i++)
            {
                _scratchList[i].Script.Updated();
            }
        }
    }
}