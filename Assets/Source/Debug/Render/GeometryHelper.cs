using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Various geometrical helpers.
    /// </summary>
    public class GeometryHelper
    {
        /// <summary>
        /// Icosahedron verts.
        /// </summary>
        private static readonly Vector3[] _IcosahedronVertices =
        {
            new Vector3(-0.525731112119133606f, 0f, 0.850650808352039932f),
            new Vector3(0.525731112119133606f, 0f, 0.850650808352039932f),
            new Vector3(-0.525731112119133606f, 0f, -0.850650808352039932f),

            new Vector3(0.525731112119133606f, 0f, -0.850650808352039932f),
            new Vector3(0f, 0.850650808352039932f, 0.525731112119133606f),
            new Vector3(0f, 0.850650808352039932f, -0.525731112119133606f),

            new Vector3(0f, -0.850650808352039932f, 0.525731112119133606f),
            new Vector3(0f, -0.850650808352039932f, -0.525731112119133606f),
            new Vector3(0.850650808352039932f, 0.525731112119133606f, 0f),

            new Vector3(-0.850650808352039932f, 0.525731112119133606f, 0f),
            new Vector3(0.850650808352039932f, -0.525731112119133606f, 0f),
            new Vector3(-0.850650808352039932f, -0.525731112119133606f, 0f)
        };

        /// <summary>
        /// Indices into icosahedron verts.
        /// </summary>
        private static readonly int[] _IcosahedronTriangles =
        {
            1,4,0,
            4,9,0,
            4,5,9,
            8,5,4,
            1,8,4,
            1,10,8,
            10,3,8,
            8,3,5,
            3,2,5,
            3,7,2,
            3,10,7,
            10,6,7,
            6,11,7,
            6,0,11,
            6,1,0,
            10,1,6,
            11,0,9,
            2,11,9,
            5,2,9,
            11,2,7
        };

        /// <summary>
        /// Constructs a sphere with equilateral triangles.
        /// </summary>
        /// <param name="subdivisions">Number of times to subdivide-- this controls the smoothness.</param>
        /// <param name="vertices">Output vertices.</param>
        /// <param name="triangles">Output triangles.</param>
        public static void GeoSphere(
            int subdivisions,
            out Vector3[] vertices, out int[] triangles)
        {
            vertices = _IcosahedronVertices;
            triangles = _IcosahedronTriangles;

            if (subdivisions > 0)
            {
                for (var i = 0; i < subdivisions; i++)
                {
                    Subdivide(ref vertices, ref triangles);
                }
            }
        }

        /// <summary>
        /// Subdivides all triangles.
        /// </summary>
        private static void Subdivide(
            ref Vector3[] vertices,
            ref int[] triangles)
        {
            // cache of midpoint indices
            var midpointIndices = new Dictionary<string, int>();

            // create lists instead...
            List<int> indexList = new List<int>(4 * triangles.Length);
            List<Vector3> vertexList = new List<Vector3>(vertices);

            // subdivide each triangle
            for (var i = 0; i < triangles.Length - 2; i += 3)
            {
                // grab indices of triangle
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                // calculate new indices
                int m01 = GetMidpointIndex(midpointIndices, vertexList, i0, i1);
                int m12 = GetMidpointIndex(midpointIndices, vertexList, i1, i2);
                int m02 = GetMidpointIndex(midpointIndices, vertexList, i2, i0);

                indexList.AddRange(
                    new[] {
                        i0,m01,m02,
                        i1,m12,m01,
                        i2,m02,m12,
                        m02,m01,m12
                    });
            }

            // save
            triangles = indexList.ToArray();
            vertices = vertexList.ToArray();
        }

        /// <summary>
        /// Used by Subdivide method.
        /// </summary>
        private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<Vector3> vertices, int i0, int i1)
        {
            // create a key
            string edgeKey = string.Format("{0}_{1}", Mathf.Min(i0, i1), Mathf.Max(i0, i1));

            int midpointIndex = -1;

            // if there is not index already...
            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                // grab the vertex values
                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];

                // calculate
                var midpoint = (v0 + v1) / 2f;

                // save
                if (vertices.Contains(midpoint))
                {
                    midpointIndex = vertices.IndexOf(midpoint);
                }
                else
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                }
            }

            return midpointIndex;
        }
    }
}