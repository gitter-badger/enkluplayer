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
                        Extents = new Vector3(1, 0, 1),
                        Position = new Vector3(3, -2, 6.4f),
                        Rotation = Quaternion.Euler(0, 45, 0)
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