using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
{
    public class StandaloneDesignController : IDesignController
    {
        private GameObject _root;

        public void Setup(DesignerContext context, IAppController app)
        {
            _root = new GameObject("Design");
            _root.AddComponent<HmdEditorKeyboardControls>();
        }

        public void Teardown()
        {
            Object.Destroy(_root);
        }

        public IAsyncToken<string> Create()
        {
            return new AsyncToken<string>(new NotImplementedException());
        }

        public void Select(string sceneId, string elementId)
        {
            
        }

        public void Focus(string sceneId, string elementId)
        {
            
        }
    }
}