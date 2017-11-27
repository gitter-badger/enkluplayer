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
                new DummyScriptManager(),
                new DummyContentAssembler(),
                data);
            return comp;
            */
        }
    }
}