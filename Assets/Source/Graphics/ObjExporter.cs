using System.Text;
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
            return Export(new ObjExporterState(objects));
        }

        /// <summary>
        /// Exports an OBJ.
        /// </summary>
        /// <param name="info">The info to export.</param>
        /// <returns></returns>
        public string Export(ObjExporterState info)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < info.Meshes.Count; i++)
            {
                var mesh = info.Meshes[i];

                builder.AppendFormat("o Object.{0}\n", i);

                WriteMeshToString(
                    mesh,
                    builder);

                builder.AppendFormat("\n");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Encodes mesh information onto a string.
        /// </summary>
        /// <param name="mesh">The <c>MeshInfo</c> to write.</param>
        /// <param name="builder">The builder to write to.</param>
        private static void WriteMeshToString(
            ObjExporterState.ObjMeshState mesh,
            StringBuilder builder)
        {
            var numVerts = mesh.Vertices.Length;

            var localToWorld = mesh.LocalToWorld;
            var vertices = mesh.Vertices;
            for (var i = 0; i < numVerts; i++)
            {
                var vertex = localToWorld.MultiplyPoint3x4(vertices[i]);
                builder.AppendFormat(
                    "v {0} {1} {2}\n",
                    vertex.x, vertex.y, vertex.z);
            }
            builder.Append("\n");

            var normals = mesh.Normals;
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

            var uvs = mesh.Uv;
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

            var triangles = mesh.Triangles;
            for (var i = 0; i < triangles.Length; i += 3)
            {
                builder.AppendFormat(
                    "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1);
            }
            
            builder.AppendLine("\n");
        }
    }
}
