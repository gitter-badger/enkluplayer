using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class AssetManifest
    {
        private readonly Dictionary<string, AssetReference> _guidToReference = new Dictionary<string, AssetReference>();
        private readonly IQueryResolver _resolver;
        private readonly IAssetLoader _loader;
        
        public AssetManifest(
            IQueryResolver resolver,
            IAssetLoader loader)
        {
            _resolver = resolver;
            _loader = loader;
        }

        public void Add(params AssetInfo[] infos)
        {
            for (int i = 0, len = infos.Length; i < len; i++)
            {
                var info = infos[i];

                _guidToReference[info.Guid] = new AssetReference(_loader, info);
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
