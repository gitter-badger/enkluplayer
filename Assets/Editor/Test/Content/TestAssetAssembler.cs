using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class TestAssetAssembler : IAssetAssembler
    {   
        public Bounds Bounds { get; private set; }
        public GameObject Assembly { get; private set; }
        
        public event Action OnAssemblyUpdated;
        
        public void Setup(Transform transform, string assetId, int version)
        {
            // Do nothing, require FinishLoad to actually invoke OnAssemblyUpdated
        }

        public void FinishLoad()
        {
            OnAssemblyUpdated.Execute();
        }

        public void Teardown()
        {
            
        }
    }
}