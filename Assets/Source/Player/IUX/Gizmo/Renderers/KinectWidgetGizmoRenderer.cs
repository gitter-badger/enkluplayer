using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Renders gizmos for Kinect.
    /// </summary>
    public class KinectWidgetGizmoRenderer : MonoBehaviourGizmoRenderer
    {
        /// <summary>
        /// Material to use.
        /// </summary>
        public Material Material;

        /// <summary>
        /// The mesh.
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// The verts.
        /// </summary>
        private Vector3[] _verts;

        /// <inheritdoc />
        public override void Initialize(Element element)
        {
            base.Initialize(element);

            _verts = new []
            {
                Vector3.zero,
                new Vector3(-4.6f, -3.2f, 4.5f),
                new Vector3(-4.6f, 3.2f, 4.5f),
                new Vector3(4.6f, 3.2f, 4.5f),
                new Vector3(4.6f, -3.2f, 4.5f)
            };

            _mesh = new Mesh
            {
                vertices = _verts,
                triangles = new[] { 0, 1, 2, 0, 2, 1, 0, 2, 3, 0, 3, 2, 0, 3, 4, 0, 4, 3, 0, 4, 1, 0, 1, 4, 1, 2, 3, 1, 3, 2, 1, 3, 4, 1, 4, 3 }
            };

            gameObject.AddComponent<MeshRenderer>().sharedMaterial = Material;
            gameObject.AddComponent<MeshFilter>().mesh = _mesh;
        }
    }
}