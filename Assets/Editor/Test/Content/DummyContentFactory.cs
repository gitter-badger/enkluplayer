using UnityEngine;

namespace CreateAR.Spire.Test
{
    public class DummyContentFactory : IContentFactory
    {
        public Content Instance(IContentManager content, ContentData data)
        {
            var go = new GameObject();
            var comp = go.AddComponent<Content>();
            comp.Setup(new DummyAssetManager(), data);
            return comp;
        }
    }
}