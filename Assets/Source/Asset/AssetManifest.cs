using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class AssetManifest
    {
        private readonly Dictionary<string, AssetReference> _guidToReference = new Dictionary<string, AssetReference>();
        private readonly IQueryResolver _resolver;
        private readonly IAssetLoader _loader;

        public event Action<AssetReference> OnNewAsset;
        public event Action<AssetReference> OnUpdatedAsset;

        public AssetManifest(
            IQueryResolver resolver,
            IAssetLoader loader)
        {
            _resolver = resolver;
            _loader = loader;
        }

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

                if (null != OnNewAsset)
                {
                    OnNewAsset(reference);
                }
            }
        }

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

                if (null != OnUpdatedAsset)
                {
                    OnUpdatedAsset(reference);
                }
            }
        }

        public AssetInfo Info(string guid)
        {
            var reference = Reference(guid);

            return null == reference
                ? null
                : reference.Info;
        }

        public AssetReference Reference(string guid)
        {
            AssetReference reference;
            _guidToReference.TryGetValue(guid, out reference);

            return reference;
        }

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
