using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer.AR
{
    /// <summary>
    /// Renders Ar planes.
    /// </summary>
    public class ArDebugRenderer : InjectableMonoBehaviour
    {
        /// <summary>
        /// Lookup from plane to gameobject representing that plane.
        /// </summary>
        private readonly Dictionary<string, GameObject> _debugPlanes = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// Provides AR implementation.
        /// </summary>
        [Inject]
        public IArService Ar { get; set; }

        /// <summary>
        /// Camnera rig.
        /// </summary>
        [Inject]
        public ArCameraRig Rig { get; set; }
        
        /// <summary>
        /// Prefab to render with.
        /// </summary>
        public GameObject Prefab;
        
        /// <inheritdoc cref="MonoBehaviour"/>
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
            for (int i = 0, len = anchors.Count; i < len; i++)
            {
                var anchor = anchors[i];

                GameObject @object;
                if (!_debugPlanes.TryGetValue(anchor.Id, out @object))
                {
                    @object = _debugPlanes[anchor.Id] = Instantiate(Prefab);

                    @object.transform.parent = Rig.transform;
                }

                var planeTransform = @object.transform;
                planeTransform.localPosition = anchor.Position.ToVector();
                planeTransform.localRotation = anchor.Rotation.ToQuaternion();
                planeTransform.localScale = anchor.Extents.ToVector();
            }
        }
    }
}
