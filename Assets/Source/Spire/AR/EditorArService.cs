using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public class EditorArService : IArService
    {
        public Camera Camera { get; set; }

        public ArAnchor[] Anchors
        {
            get
            {
                return new []
                {
                    new ArAnchor("floor")
                    {
                        Center = Vector3.zero,
                        Extents = new Vector3(float.MaxValue, 0, float.MaxValue),
                        Position = Vector3.zero,
                        Rotation = Quaternion.identity
                    }, 
                };
            }
        }

        public void Setup(ArServiceConfiguration config)
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}