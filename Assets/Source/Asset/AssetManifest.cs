using System;
using System.Collections.Generic;

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
        private readonly Dictionary<string, AssetReference> _guidToReference = new Dictionary<string, AssetReference>();

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
        public event Action<AssetReference> OnAssetAdded;

        /// <summary>
        /// Called when an asset has been updated.
        /// </summary>
        public event Action<AssetReference> OnAssetUpdated;

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
        /// <param name="infos">One or more <c>AssetInfo</c> instances to add.</param>
        public void Add(params AssetInfo[] infos)
        {
            if (null == infos)
            {
                throw new ArgumentException("Cannot add null.");
            }

            // validate
            for (int i = 0, len = infos.Length; i < len; i++)
            {
                if (_guidToReference.ContainsKey(infos[i].Guid))
                {
                    throw new ArgumentException("Cannot add AssetInfo with same Guid as previous asset : " + infos[i].Guid);
                }
            }

            // add
            for (int i = 0, len = infos.Length; i < len; i++)
            {
                var info = infos[i];
                var reference = new AssetReference(_loader, info);

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
        /// <param name="infos"></param>
        public void Update(params AssetInfo[] infos)
        {
            if (null == infos)
            {
                throw new ArgumentException("Cannot update null.");
            }

            for (int i = 0, len = infos.Length; i < len; i++)
            {
                var guid = infos[i].Guid;
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentException("Cannot update with null or empty Guid.");
                }

                if (!_guidToReference.ContainsKey(guid))
                {
                    throw new ArgumentException("Cannot update non-existent Asset : " + guid);
                }
            }

            for (int i = 0, len = infos.Length; i < len; i++)
            {
                var info = infos[i];
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
        public AssetInfo Info(string guid)
        {
            var reference = Reference(guid);

            return null == reference
                ? null
                : reference.Info;
        }

        /// <summary>
        /// Retrieves the <c>AssetReference</c> for a particular guid.
        /// </summary>
        /// <param name="guid">The guid for a particular asset.</param>
        /// <returns></returns>
        public AssetReference Reference(string guid)
        {
            AssetReference reference;
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
        public AssetReference FindOne(string query)
        {
            foreach (var pair in _guidToReference)
            {
                if (_resolver.Resolve(
                    query,
                    ref pair.Value.Info.Tags))
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
        public AssetReference[] Find(string query)
        {
            var references = new List<AssetReference>();
            foreach (var pair in _guidToReference)
            {
                if (_resolver.Resolve(
                    query,
                    ref pair.Value.Info.Tags))
                {
                    references.Add(pair.Value);
                }
            }

            return references.ToArray();
        }
    }
}