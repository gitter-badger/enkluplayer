using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Stores the state of a mesh hierarchy.
    /// </summary>
    public class ObjExporterState
    {
        /// <summary>
        /// Stores the state of a mesh.
        /// </summary>
        public class ObjMeshState
        {
            /// <summary>
            /// Mesh information.
            /// </summary>
            public readonly Matrix4x4 LocalToWorld;
            public readonly Vector3[] Vertices;
            public readonly Vector3[] Normals;
            public readonly Vector2[] Uv;
            public readonly int[] Triangles;

            /// <summary>
            /// Constructor.
            /// </summary>
            public ObjMeshState(MeshFilter filter)
            {
                LocalToWorld = filter.transform.localToWorldMatrix;

                var mesh = UnityEngine.Application.isPlaying
                    ? filter.mesh
                    : filter.sharedMesh;

                Vertices = mesh.vertices;
                Normals = mesh.normals;
                Uv = mesh.uv;
                Triangles = mesh.triangles;
            }
        }

        /// <summary>
        /// Set of meshes.
        /// </summary>
        public readonly List<ObjMeshState> Meshes = new List<ObjMeshState>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjExporterState(GameObject[] gameObjects)
        {
            var filters = gameObjects
                .SelectMany(@object => @object.GetComponentsInChildren<MeshFilter>())
                .ToArray();
            for (int i = 0, len = filters.Length; i < len; i++)
            {
                Meshes.Add(new ObjMeshState(filters[i]));
            }
        }
    }
}