using UnityEngine;

namespace CreateAR.Spire.Test
{
    public class DummyContentFactory : IContentFactory
    {
        public Content Instance(ContentData data)
        {
            var go = new GameObject();
            var comp = go.AddComponent<Content>();
            comp.Setup(new DummyAssetManager(), data);
            return comp;
        }
    }
}