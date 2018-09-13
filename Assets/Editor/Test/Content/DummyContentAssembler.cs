using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test
{
    public class DummyContentAssembler : IContentAssembler
    {
        public Bounds Bounds { get; private set; }

        private MutableAsyncToken<GameObject> _onAssemblyComplete = new MutableAsyncToken<GameObject>();

        public IMutableAsyncToken<GameObject> OnAssemblyComplete
        {
            get { return _onAssemblyComplete; }
        }

        public void Setup(Transform transform, string assetId)
        {
            _onAssemblyComplete.Succeed(null);
        }

        public void Teardown()
        {
            
        }
    }
}