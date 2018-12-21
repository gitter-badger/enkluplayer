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
        /// Vertices for octohedron.
        /// </summary>
        private static readonly Vector3[] _OctohedronVertices =
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        /// <summary>
        /// Indices into octohedron verts.
        /// </summary>
        private static readonly int[] _OctohedronTriangles =
        {
            4, 0, 2,
            4, 2, 1,
            4, 1, 3,
            4, 3, 0,
            5, 2, 0,
            5, 1, 2,
            5, 3, 1,
            5, 0, 3
        };

        /// <summary>
        /// Constants needed for dodecahedron construction.
        /// </summary>
        private static readonly float A = 1f / Mathf.Sqrt(3);
        private static readonly float B = Mathf.Sqrt((3 - Mathf.Sqrt(5)) / 6);
        private static readonly float C = Mathf.Sqrt((3 + Mathf.Sqrt(5)) / 6);

        /// <summary>
        /// Verts for dodecahedron.
        /// </summary>
        private static readonly Vector3[] _DodecahedronVerts =
        {
            new Vector3(A, A, A),
            new Vector3(A, A, -A),
            new Vector3(A, -A, A),
            new Vector3(A, -A, -A),

            new Vector3(-A, A, A),
            new Vector3(-A, A, -A),
            new Vector3(-A, -A, A),
            new Vector3(-A, -A, -A),

            new Vector3(B, C, 0),
            new Vector3(-B, C, 0),
            new Vector3(B, -C, 0),
            new Vector3(-B, -C, 0),

            new Vector3(C, 0, B),
            new Vector3(C, 0, -B),
            new Vector3(-C, 0, B),
            new Vector3(-C, 0, -B),

            new Vector3(0, B, C),
            new Vector3(0, -B, C),
            new Vector3(0, B, -C),
            new Vector3(0, -B, -C)
        };

        /// <summary>
        /// Indices into dodecahedron verts.
        /// </summary>
        private static readonly int[] _DodecahedronTriangles =
        {
            0, 8, 9, 0, 9, 4, 0, 4, 16,
            0, 16, 17, 0, 17, 2, 0, 2, 12,
            12, 2, 10, 12, 10, 3, 12, 3, 13,
            9, 5, 15, 9, 15, 14, 9, 14, 4,
            3, 19, 18, 3, 18, 1, 3, 1, 13,
            7, 11, 6, 7, 6, 14, 7, 14, 15,

            0, 12, 13, 0, 13, 1, 0, 1, 8,
            8, 1, 18, 8, 18, 5, 8, 5, 9,
            16, 4, 14, 16, 14, 6, 16, 6, 17,
            6, 11, 10, 6, 10, 2, 6, 2, 17,
            7, 15, 5, 7, 5, 18, 7, 18, 19,
            7, 19, 3, 7, 3, 10, 7, 10, 11
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

            for (int i = 0, len = vertices.Length; i < len; i++)
            {
                vertices[i] = vertices[i].normalized;
            }
        }

        /// <summary>
        /// Constructs an octohedron.
        /// </summary>
        /// <param name="vertices">Output vertices.</param>
        /// <param name="triangles">Output triangles.</param>
        public static void Octohedron(
            out Vector3[] vertices,
            out int[] triangles)
        {
            vertices = _OctohedronVertices;
            triangles = _OctohedronTriangles;
        }

        /// <summary>
        /// Constructs a dodecahedron.
        /// </summary>
        /// <param name="vertices">Output verts.</param>
        /// <param name="triangles">Output triangles.</param>
        public static void Dodecahedron(
            out Vector3[] vertices,
            out int[] triangles)
        {
            vertices = _DodecahedronVerts;
            triangles = _DodecahedronTriangles;
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