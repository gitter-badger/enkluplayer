using System;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that can export to OBJ.
    /// </summary>
    public class ObjExporter
    {
        /// <summary>
        /// Exports a set of GameObjects as a single OBJ.
        /// </summary>
        /// <param name="objects">The GameObjects to export.</param>
        /// <returns></returns>
        public string Export(params GameObject[] objects)
        {
            var instances = objects
                .SelectMany(@object => @object.GetComponentsInChildren<MeshFilter>())
                .Select(filter =>
                {
                    var mesh = UnityEngine.Application.isPlaying
                        ? filter.mesh
                        : filter.sharedMesh;
                    return new CombineInstance
                    {
                        mesh = mesh,
                        transform = filter.transform.localToWorldMatrix
                    };
                })
                .ToArray();
            
            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(instances, true, true, false);
            
            var builder = new StringBuilder();
            builder.AppendLine("o Object.0");
            WriteMeshToString(
                combinedMesh,
                Matrix4x4.identity,
                builder);
            return builder.ToString();
        }

        /// <summary>
        /// Encodes a mesh as an OBJ.
        /// </summary>
        /// <param name="mesh">The mesh in question.</param>
        /// <param name="localToWorld">Local to world matrix.</param>
        /// <param name="builder">The builder to write to.</param>
        private static void WriteMeshToString(
            Mesh mesh,
            Matrix4x4 localToWorld,
            StringBuilder builder)
        {
            var numVerts = mesh.vertexCount;

            var vertices = mesh.vertices;
            for (var i = 0; i < numVerts; i++)
            {
                var vertex = localToWorld.MultiplyPoint3x4(vertices[i]);
                builder.AppendFormat(
                    "v {0} {1} {2}\n",
                    vertex.x, vertex.y, vertex.z);
            }
            builder.Append("\n");

            var normals = mesh.normals;
            if (normals.Length == numVerts)
            {
                for (var i = 0; i < numVerts; i++)
                {
                    var normal = normals[i];
                    builder.AppendFormat(
                        "vn {0} {1} {2}\n",
                        normal.x, normal.y, normal.z);
                }
                builder.Append("\n");
            }

            var uvs = mesh.uv;
            if (uvs.Length == numVerts)
            {
                for (var i = 0; i < numVerts; i++)
                {
                    var uv = uvs[i];
                    builder.AppendFormat(
                        "vt {0} {1}\n",
                        uv.x, uv.y);
                }
                builder.Append("\n");
            }

            try
            {
                for (var material = 0; material < mesh.subMeshCount; material++)
                {
                    var triangles = mesh.GetTriangles(material);
                    for (var i = 0; i < triangles.Length; i += 3)
                    {
                        builder.AppendFormat(
                            "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                            triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(mesh, "Could not write triangles : {0}.", exception);
            }

            builder.AppendLine("\n");
        }
    }
}
