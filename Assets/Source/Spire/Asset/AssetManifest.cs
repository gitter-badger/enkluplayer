using System;
using System.Collections.Generic;
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Serves as the table of contets for all Assets.
    /// </summary>
    public class AssetManifest
    {
        /// <summary>
        /// A lookup from guid to AssetReference.
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
        /// Called when an asset has been added.
        /// </summary>
        public event Action<Asset> OnAssetAdded;

        /// <summary>
        /// Called when an asset has been updated.
        /// </summary>
        public event Action<Asset> OnAssetUpdated;

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
        /// <param name="assets>One or more <c>AssetInfo</c> instances to add.</param>
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
        /// Updates a set of <c>AssetInfo</c> instances. Throws an <c>ArgumentException</c>
        /// if an <c>AssetInfo</c> does not exist.
        /// </summary>
        /// <param name="assets></param>
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
                var reference = Reference(info.Guid);
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
        public AssetData Info(string guid)
        {
            var reference = Reference(guid);

            return null == reference
                ? null
                : reference.Data;
        }

        /// <summary>
        /// Retrieves the <c>AssetReference</c> for a particular guid.
        /// </summary>
        /// <param name="guid">The guid for a particular asset.</param>
        /// <returns></returns>
        public Asset Reference(string guid)
        {
            Asset reference;
            _guidToReference.TryGetValue(guid, out reference);

            return reference;
        }

        /// <summary>
        /// Finds a single <c>AssetReference</c> by some query.
        /// 
        /// Queries are resolved against <c>AssetReference</c> tags via the
        /// <c>IQueryResolver</c> object passed into the
        /// <c>AssetManagerConfiguration.</c>
        /// </summary>
        /// <param name="query">The query to resolve.</param>
        /// <returns></returns>
        public Asset FindOne(string query)
        {
            foreach (var pair in _guidToReference)
            {
                if (_resolver.Resolve(
                    query,
                    ref pair.Value.Data.Tags))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all <c>AssetReference</c> instances that match the given query.
        /// 
        /// Queries are resolved against <c>AssetReference</c> tags via the
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
                if (_resolver.Resolve(
                    query,
                    ref pair.Value.Data.Tags))
                {
                    references.Add(pair.Value);
                }
            }

            return references.ToArray();
        }
    }
}