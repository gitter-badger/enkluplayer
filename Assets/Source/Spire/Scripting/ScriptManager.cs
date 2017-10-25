﻿using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates, queries, and destroys <c>SpireScript</c> instances.
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
            public readonly SpireScript Script;

            /// <summary>
            /// Set of unique tags. These are used to cleanup references.
            /// </summary>
            public readonly HashSet<string> Tags = new HashSet<string>();

            /// <summary>
            /// Creates a new record.
            /// </summary>
            /// <param name="script">The script instance.</param>
            /// <param name="tags">Tags associated with creation.</param>
            public ScriptRecord(SpireScript script, params string[] tags)
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
                    Tags.Add(tags[i]);
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
                    Tags.Remove(tags[i]);
                }
            }
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAppDataManager _appData;
        private readonly IAssetManager _assets;
        private readonly IScriptParser _parser;
        private readonly IQueryResolver _resolver;

        /// <summary>
        /// Tracks all scripts.
        /// </summary>
        private readonly List<ScriptRecord> _records = new List<ScriptRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appData">App data.</param>
        /// <param name="assets">Assets.</param>
        /// <param name="parser">For parsing scripts, asynchronously.</param>
        /// <param name="resolver">Resolves queries.</param>
        public ScriptManager(
            IAppDataManager appData,
            IAssetManager assets,
            IScriptParser parser,
            IQueryResolver resolver)
        {
            _appData = appData;
            _assets = assets;
            _parser = parser;
            _resolver = resolver;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public SpireScript FindOne(string id)
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
        public void FindAll(string id, List<SpireScript> scripts)
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
        public SpireScript FindOneTagged(string query)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (_resolver.Resolve(query, ref record.Script.Data.Tags))
                {
                    return record.Script;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void FindAllTagged(string query, List<SpireScript> scripts)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (_resolver.Resolve(query, ref record.Script.Data.Tags))
                {
                    scripts.Add(record.Script);
                }
            }
        }

        /// <inheritdoc cref="IScriptManager"/>
        public SpireScript Create(string scriptId, params string[] tags)
        {
            var data = _appData.Get<ScriptData>(scriptId);
            if (null == data)
            {
                Log.Warning(this,
                    "Could not find ScriptData by id {0}.",
                    scriptId);
                return null;
            }

            var asset = _assets.Manifest.Asset(data.Asset.AssetDataId);
            if (null == asset)
            {
                Log.Warning(this,
                    "Could not find asset by id {0}.", data.Asset.AssetDataId);
                return null;
            }

            var script = new SpireScript(_parser, data, asset);

            _records.Add(new ScriptRecord(script, tags));

            return script;
        }

        /// <inheritdoc cref="IScriptManager"/>
        public void Release(SpireScript script)
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

                if (0 == record.Tags.Count)
                {
                    record.Script.Release();

                    _records.RemoveAt(i);
                }
            }
        }
    }
}