using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class SceneUIElementFactory : MonoBehaviour, IUIElementFactory
    {
        [Serializable]
        public class ElementLink
        {
            public string UIDataId;
            public MonoBehaviourUIElement Element;
        }

        [Serializable]
        public class ElementLinkLibrary
        {
            public RuntimePlatform[] Platforms;

            public ElementLink[] Links;
        }

        public ElementLinkLibrary[] Libraries;

        public IAsyncToken<IUIElement> Element(UIReference reference, uint id)
        {
            throw new NotImplementedException();
        }
    }
}