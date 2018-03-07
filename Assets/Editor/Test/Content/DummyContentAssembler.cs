using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyContentAssembler : IContentAssembler
    {
        public Bounds Bounds { get; private set; }
        public event Action<GameObject> OnAssemblyComplete;

        public void Setup(ContentData data)
        {
            if (null != OnAssemblyComplete)
            {
                OnAssemblyComplete(null);
            }
        }

        public void UpdateMaterialData(MaterialData material)
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}