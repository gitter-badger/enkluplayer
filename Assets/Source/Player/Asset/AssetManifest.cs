using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Serves as the table of contents for all Assets.
    /// </summary>
    public class AssetManifest
    {
        /// <summary>
        /// Used for internal record-keeping. Holds at least the latest
        /// <c>Asset</c>, lazily creating previous versions are requested.
        /// </summary>
        private class AssetRecord
        {
            /// <summary>
            /// The loader passed to Assets.
            /// </summary>
            private readonly IAssetLoader _loader;

            /// <summary>
            /// Watchers.
            /// </summary>
            private readonly List<Action<AssetData>> _watchUpdate = new List<Action<AssetData>>();
            private readonly List<Action> _watchRemove = new List<Action>();

            /// <summary>
            /// List of references. Latest first.
            /// </summary>
            public readonly List<Asset> References = new List<Asset>();

            /// <summary>
            /// Data for the latest version.
            /// </summary>
            public AssetData Data { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public AssetRecord(IAssetLoader loader, AssetData data)
            {
                _loader = loader;

                Update(data);
            }

            /// <summary>
            /// Updates record with the latest version of data.
            /// </summary>
            /// <param name="data">The latest data.</param>
            public void Update(AssetData data)
            {
                Data = data;
                References.Insert(0, new Asset(_loader, data, data.Version));

                var copy = _watchUpdate.ToArray();
                for (int i = 0, len = copy.Length; i < len; i++)
                {
                    copy[i](data);
                }
            }

            /// <summary>
            /// Retrieves an asset for a specific version of data.
            /// </summary>
            /// <param name="version">The version.</param>
            /// <returns></returns>
            public Asset Version(int version)
            {
                if (-1 == version)
                {
                    return References[0];
                }

                if (version > Data.Version)
                {
                    return null;
                }

                for (int i = 0, len = References.Count; i < len; i++)
                {
                    var reference = References[i];
                    if (reference.Version == version)
                    {
                        return reference;
                    }
                }

                var asset = new Asset(_loader, Data, version);
                References.Add(asset);

                return asset;
            }

            /// <summary>
            /// Destroys all references.
            /// </summary>
            public void Remove()
            {
                foreach (var asset in References)
                {
                    asset.Unload();
                }

                References.Clear();

                var copy = _watchRemove.ToArray();
                for (int i = 0, len = copy.Length; i < len; i++)
                {
                    copy[i]();
                }
            }

            /// <summary>
            /// Watches for updates.
            /// </summary>
            /// <param name="callback">The callback.</param>
            /// <returns></returns>
            public Action Watch(Action<AssetData> callback)
            {
                _watchUpdate.Add(callback);

                return () => _watchUpdate.Remove(callback);
            }

            /// <summary>
            /// Watches for removes.
            /// </summary>
            /// <param name="callback">The callback.</param>
            /// <returns></returns>
            public Action WatchRemove(Action callback)
            {
                _watchRemove.Add(callback);

                return () => _watchRemove.Remove(callback);
            }
        }

        /// <summary>
        /// A lookup from guid to AssetRecord.
        /// </summary>
        private readonly Dictionary<string, AssetRecord> _guidToRecord = new Dictionary<string, AssetRecord>();

        /// <summary>
        /// Resolves queries against tags.
        /// </summary>
        private readonly IQueryResolver _resolver;

        /// <summary>
        /// Loads assets.
        /// </summary>
        private readonly IAssetLoader _loader;

        /// <summary>
        /// Retrieves all asset data. Creates a copy of the internal data structure,
        /// thus-- this should be treated with care.
        /// </summary>
        public AssetData[] All
        {
            get
            {
                return _guidToRecord.Values.Select(rec => rec.Data).ToArray();
            }
        }

        /// <summary>
        /// Called when an asset has been added.
        /// </summary>
        public event Action<AssetData> OnAssetAdded;

        /// <summary>
        /// Called when an asset has been updated.
        /// </summary>
        public event Action<AssetData> OnAssetUpdated;

        /// <summary>
        /// Called when an asset has been removed.
        /// </summary>
        public event Action<AssetData> OnAssetRemoved;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resolver">Resolves tag queries.</param>
        /// <param name="loader">Loads assets.</param>
        public AssetManifest(
            IQueryResolver resolver,
            IAssetLoader loader)
        {
            _resolver = resolver;
            _loader = loader;
        }

        /// <summary>
        /// Destroys the <c>AssetManifest</c>.
        /// </summary>
        public void Destroy()
        {
            // destroy
            foreach (var record in _guidToRecord.Values)
            {
                foreach (var reference in record.References)
                {
                    reference.Unload();
                }
            }
            _guidToRecord.Clear();
        }

        /// <summary>
        /// Adds a set of assets. Throws an <c>ArgumentException</c> if an <c>AssetInfo</c>
        /// instance has a guid that matches an existing instance.
        /// </summary>
        /// <param name="assets">One or more <c>AssetInfo</c> instances to add.</param>
        public void Add(params AssetData[] assets)
        {
            if (null == assets)
            {
                throw new ArgumentException("Cannot add null.");
            }

            // validate
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                if (_guidToRecord.ContainsKey(assets[i].Guid))
                {
                    throw new ArgumentException("Cannot add AssetInfo with same Guid as previous asset : " + assets[i].Guid);
                }
            }

            // add
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var data = assets[i];
                var record = new AssetRecord(_loader, data);

                _guidToRecord[data.Guid] = record;

                if (null != OnAssetAdded)
                {
                    OnAssetAdded(data);
                }
            }
        }

        /// <summary>
        /// Removes assets from manifest.
        /// </summary>
        /// <param name="assetIds">The ids of assets to remove.</param>
        public void Remove(params string[] assetIds)
        {
            if (null == assetIds)
            {
                throw new Exception("Cannot remove null.");
            }

            for (int i = 0, len = assetIds.Length; i < len; i++)
            {
                var assetId = assetIds[i];

                AssetRecord record;
                if (_guidToRecord.TryGetValue(assetId, out record))
                {
                    record.Remove();

                    _guidToRecord.Remove(assetId);

                    if (null != OnAssetRemoved)
                    {
                        OnAssetRemoved(record.Data);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a set of <c>AssetInfo</c> instances. Throws an <c>ArgumentException</c>
        /// if an <c>AssetInfo</c> does not exist.
        /// </summary>
        /// <param name="assets">Assets to update.</param>
        public void Update(params AssetData[] assets)
        {
            if (null == assets)
            {
                throw new ArgumentException("Cannot update null.");
            }

            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var guid = assets[i].Guid;
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentException("Cannot update with null or empty Guid.");
                }

                if (!_guidToRecord.ContainsKey(guid))
                {
                    throw new ArgumentException("Cannot update non-existent Asset : " + guid);
                }
            }

            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var data = assets[i];

                AssetRecord record;
                if (!_guidToRecord.TryGetValue(data.Guid, out record))
                {
                    throw new ArgumentException("Invalid AssetData update refers to asset that does not exist.");
                }

                record.Update(data);

                if (null != OnAssetUpdated)
                {
                    OnAssetUpdated(record.Data);
                }
            }
        }

        /// <summary>
        /// Retrieves the <c>AssetInfo</c> for a specific guid.
        /// </summary>
        /// <param name="guid">The guid for a particular asset.</param>
        /// <returns></returns>
        public AssetData Data(string guid)
        {
            AssetRecord record;
            if (_guidToRecord.TryGetValue(guid, out record))
            {
                return record.Data;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the <c>Asset</c> for a particular guid.
        /// </summary>
        /// <param name="guid">The guid for a particular asset.</param>
        /// <param name="version">An optional version or -1 for latest.</param>
        /// <returns></returns>
        public Asset Asset(string guid, int version)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            AssetRecord record;
            if (_guidToRecord.TryGetValue(guid, out record))
            {
                return record.Version(version);
            }

            return null;
        }

        /// <summary>
        /// Finds a single <c>Asset</c> by some query.
        /// 
        /// Queries are resolved against <c>Asset</c> tags via the
        /// <c>IQueryResolver</c> object passed into the
        /// <c>AssetManagerConfiguration.</c>
        /// </summary>
        /// <param name="query">The query to resolve.</param>
        /// <param name="version">Version of results or -1 for latest.</param>
        /// <returns></returns>
        public Asset FindOne(string query, int version)
        {
            foreach (var pair in _guidToRecord)
            {
                var tagsArray = pair.Value.Data.Tags.Split(',');
                if (_resolver.Resolve(
                    query,
                    ref tagsArray))
                {
                    return pair.Value.Version(version);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all <c>Asset</c> instances that match the given query.
        /// 
        /// Queries are resolved against <c>Asset</c> tags via the
        /// <c>IQueryResolver</c> object passed into the
        /// <c>AssetManagerConfiguration.</c>
        /// </summary>
        /// <param name="query">The query to resolve.</param>
        /// <param name="version">Version of results or -1 for latest.</param>
        /// <returns></returns>
        public Asset[] Find(string query, int version)
        {
            var references = new List<Asset>();
            foreach (var pair in _guidToRecord)
            {
                var tagsArray = pair.Value.Data.Tags.Split(',');
                if (_resolver.Resolve(
                    query,
                    ref tagsArray))
                {
                    references.Add(pair.Value.Version(version));
                }
            }

            return references.ToArray();
        }

        /// <summary>
        /// Watches for changes to an asset.
        /// </summary>
        /// <param name="assetId">The asset id to watch.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public Action WatchUpdate(string assetId, Action<AssetData> callback)
        {
            AssetRecord record;
            if (_guidToRecord.TryGetValue(assetId, out record))
            {
                return record.Watch(callback);
            }

            return () => { };
        }

        public Action WatchRemove(string assetId, Action callback)
        {
            AssetRecord record;
            if (_guidToRecord.TryGetValue(assetId, out record))
            {
                return record.WatchRemove(callback);
            }

            return () => { };
        }
    }
}