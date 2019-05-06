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

            var controls = _root.AddComponent<StandaloneMenuControls>();
            controls.OnMenu += Controls_OnMenu;
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
            // do nothing
        }

        public void Focus(string sceneId, string elementId)
        {
            // do nothing
        }

        private void Controls_OnMenu()
        {
            if (null != _root.GetComponent<StandaloneMenuViewController>())
            {
                return;
            }

            _root.AddComponent<StandaloneMenuViewController>();
        }
    }
}