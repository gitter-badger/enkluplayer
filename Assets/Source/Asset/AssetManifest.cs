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

        public AssetManifest(
            IQueryResolver resolver,
            IAssetLoader loader)
        {
            _resolver = resolver;
            _loader = loader;
        }

        public void Add(params AssetInfo[] infos)
        {
            // validate
            for (int i = 0, len = infos.Length; i < len; i++)
            {
                if (_guidToReference.ContainsKey(infos[i].Guid))
                {
                    throw new Exception("Cannot add AssetInfo with same Guid as previous asset.");
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
