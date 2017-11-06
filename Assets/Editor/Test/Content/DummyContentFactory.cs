using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyContentFactory : IContentFactory
    {
        public SpirePlayer.Content Instance(IContentManager content, ContentData data)
        {
            var go = new GameObject();
            var comp = go.AddComponent<SpirePlayer.Content>();
            comp.Setup(
                new DummyAssetManager(),
                new DummyScriptManager(),
                new DummyPoolManager(), 
                data);
            return comp;
        }
    }
}