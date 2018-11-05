using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test
{
    public class DummyAssetAssembler : IAssetAssembler
    {
        public Bounds Bounds { get; private set; }
        public GameObject Assembly { get; private set; }

        public event Action OnAssemblyUpdated;

        public void Setup(Transform transform, string assetId, int version)
        {
            Assembly = new GameObject(assetId);

            if (null != OnAssemblyUpdated)
            {
                OnAssemblyUpdated();
            }
        }

        public void Teardown()
        {
            if (null != Assembly)
            {
                UnityEngine.Object.DestroyImmediate(Assembly);
            }
        }
    }
}