using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public class EditorArService : IArService
    {
        public ArAnchor[] Anchors
        {
            get
            {
                return new []
                {
                    new ArAnchor("floor")
                    {
                        Center = Vector3.zero,
                        Extents = new Vector3(100, 0, 100),
                        Position = Vector3.zero,
                        Rotation = Quaternion.identity
                    }, 
                };
            }
        }

        public ArServiceConfiguration Config { get; private set; }

        public void Setup(ArServiceConfiguration config)
        {
            Config = config;
        }

        public void Teardown()
        {
            
        }
    }
}