using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyContentAssembler : IContentAssembler
    {
        public Bounds Bounds { get; private set; }
        public GameObject Assembly { get; private set; }
        public event Action<GameObject> OnAssemblyComplete;
        
        public void Setup(Transform transform, string assetId)
        {
            if (null != OnAssemblyComplete)
            {
                OnAssemblyComplete(null);
            }
        }

        public void Teardown()
        {
            
        }
    }
}