using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Serves as the table of contets for all Assets.
    /// </summary>
    public class AssetManifest
    {
        /// <summary>
        /// A lookup from guid to Asset.
        /// </summary>
        private readonly Dictionary<string, Asset> _guidToReference = new Dictionary<string, Asset>();

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
                return _guidToReference
                    .Values
                    .Select(asset => asset.Data)
                    .ToArray();
            }
        }

        /// <summary>
        /// Called when an asset has been added.
        /// </summary>
        public event Action<Asset> OnAssetAdded;

        /// <summary>
        /// Called when an asset has been updated.
        /// </summary>
        public event Action<Asset> OnAssetUpdated;

        /// <summary>
        /// Called when an asset has been removed.
        /// </summary>
        public event Action<Asset> OnAssetRemoved;

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
                if (_guidToReference.ContainsKey(assets[i].Guid))
                {
                    throw new ArgumentException("Cannot add AssetInfo with same Guid as previous asset : " + assets[i].Guid);
                }
            }

            // add
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var info = assets[i];
                var reference = new Asset(_loader, info);

                _guidToReference[info.Guid] = reference;

                if (null != OnAssetAdded)
                {
                    OnAssetAdded(reference);
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
                var asset = Asset(assetId);
                if (null != asset)
                {
                    _guidToReference.Remove(assetId);

                    asset.Remove();

                    if (null != OnAssetRemoved)
                    {
                        OnAssetRemoved(asset);
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

                if (!_guidToReference.ContainsKey(guid))
                {
                    throw new ArgumentException("Cannot update non-existent Asset : " + guid);
                }
            }

            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var info = assets[i];
                var reference = Asset(info.Guid);
                if (null == reference)
                {
                    throw new ArgumentException("Invalid AssetInfo.");
                }

                reference.Update(info);

                if (null != OnAssetUpdated)
                {
                    OnAssetUpdated(reference);
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
            var reference = Asset(guid);

            return null == reference
                ? null
                : reference.Data;
        }

        /// <summary>
        /// Retrieves the <c>Asset</c> for a particular guid.
        /// </summary>
        /// <param name="guid">The guid for a particular asset.</param>
        /// <returns></returns>
        public Asset Asset(string guid)
        {
            Asset reference;
            _guidToReference.TryGetValue(guid, out reference);

            return reference;
        }

        /// <summary>
        /// Finds a single <c>Asset</c> by some query.
        /// 
        /// Queries are resolved against <c>Asset</c> tags via the
        /// <c>IQueryResolver</c> object passed into the
        /// <c>AssetManagerConfiguration.</c>
        /// </summary>
        /// <param name="query">The query to resolve.</param>
        /// <returns></returns>
        public Asset FindOne(string query)
        {
            foreach (var pair in _guidToReference)
            {
                var tagsArray = pair.Value.Data.Tags.Split(',');
                if (_resolver.Resolve(
                    query,
                    ref tagsArray))
                {
                    return pair.Value;
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
        /// <returns></returns>
        public Asset[] Find(string query)
        {
            var references = new List<Asset>();
            foreach (var pair in _guidToReference)
            {
                var tagsArray = pair.Value.Data.Tags.Split(',');
                if (_resolver.Resolve(
                    query,
                    ref tagsArray))
                {
                    references.Add(pair.Value);
                }
            }

            return references.ToArray();
        }
    }
}