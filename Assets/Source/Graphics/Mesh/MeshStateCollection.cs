using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Stores the state of a mesh hierarchy.
    /// </summary>
    public class MeshStateCollection
    {
        /// <summary>
        /// Stores the state of a mesh.
        /// </summary>
        public class MeshState
        {
            /// <summary>
            /// Mesh information.
            /// </summary>
            public Matrix4x4 LocalToWorld;
            public Vector3[] Vertices;
            public int[] Triangles;

            public int NumVerts;
            public int NumTris;

            /// <summary>
            /// Constructor.
            /// </summary>
            public MeshState(MeshFilter filter)
            {
                LocalToWorld = filter.transform.localToWorldMatrix;

                var mesh = UnityEngine.Application.isPlaying
                    ? filter.mesh
                    : filter.sharedMesh;

                Vertices = mesh.vertices;
                Triangles = mesh.triangles;

                NumVerts = Vertices.Length;
                NumTris = Triangles.Length / 3;
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            public MeshState()
            {
                //
            }
        }

        /// <summary>
        /// Set of meshes.
        /// </summary>
        public readonly List<MeshState> Meshes = new List<MeshState>();

        /// <summary>
        /// Counts triangles.
        /// </summary>
        public int Triangles
        {
            get
            {
                var acc = 0;
                for (var i = 0; i < Meshes.Count; i++)
                {
                    var state = Meshes[i];
                    acc += state.NumTris;
                }

                return acc;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshStateCollection(GameObject[] gameObjects)
        {
            var filters = gameObjects
                .Where(@object => null != @object)
                .SelectMany(@object => @object.GetComponentsInChildren<MeshFilter>())
                .ToArray();
            for (int i = 0, len = filters.Length; i < len; i++)
            {
                Meshes.Add(new MeshState(filters[i]));
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshStateCollection()
        {
            //
        }
    }
}