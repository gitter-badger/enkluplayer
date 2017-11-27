using CreateAR.SpirePlayer.Test.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyContentFactory : IContentFactory
    {
        public SpirePlayer.Content Instance(IContentManager content, ContentData data)
        {
            return null;
            /*
            var go = new GameObject();
            var comp = go.AddComponent<SpirePlayer.Content>();
            comp.Setup(
                null,
                new DummyAssetManager(),
                new DummyScriptManager(),
                new DummyPoolManager(), 
                data);
            return comp;
            */
        }
    }
}