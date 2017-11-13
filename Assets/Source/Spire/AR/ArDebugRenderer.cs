using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public class ArDebugRenderer : InjectableMonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _debugPlanes = new Dictionary<string, GameObject>();
        
        [Inject]
        public IArService Ar { get; set; }

        public GameObject Prefab;
        
        private void Update()
        {
            if (null == Ar.Config || !Ar.Config.DrawPlanes)
            {
                foreach (var pair in _debugPlanes)
                {
                    Destroy(pair.Value);
                }
                
                _debugPlanes.Clear();
            
                return;
            }

            var anchors = Ar.Anchors;
            for (int i = 0, len = anchors.Length; i < len; i++)
            {
                var anchor = anchors[i];

                GameObject @object;
                if (!_debugPlanes.TryGetValue(anchor.Id, out @object))
                {
                    @object = _debugPlanes[anchor.Id] = Instantiate(Prefab);
                }

                var planeTransform = @object.transform;
                planeTransform.position = anchor.Position;
                planeTransform.rotation = anchor.Rotation;
                planeTransform.localScale = anchor.Extents;
            }
        }
    }
}