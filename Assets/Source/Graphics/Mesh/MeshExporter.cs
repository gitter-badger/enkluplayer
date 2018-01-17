using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that can export a mesh.
    /// </summary>
    public class MeshExporter
    {
        /// <summary>
        /// Exports a set of GameObjects as a single OBJ.
        /// </summary>
        /// <param name="objects">The GameObjects to export.</param>
        /// <returns></returns>
        public byte[] Export(params GameObject[] objects)
        {
            return Export(new MeshStateCollection(objects));
        }

        /// <summary>
        /// Exports an OBJ.
        /// </summary>
        /// <param name="info">The info to export.</param>
        /// <returns></returns>
        public byte[] Export(MeshStateCollection info)
        {
            var size = 0;
            var meshes = info.Meshes;
            
            // count so we can just allocate a single buffer
            for (int i = 0, len = meshes.Count; i < len; i++)
            {
                size += 8;                          // header   -- two ints
                size += meshes[i].NumVerts * 3 * 4; // verts    -- three floats
                size += meshes[i].NumTris * 3 * 2;  // tris     -- three ints
            }
            
            // allocate
            var buffer = new byte[size];

            // write meshes
            var index = 0;
            for (int i = 0, len = meshes.Count; i < len; i++)
            {
                var mesh = meshes[i];

                // pack header
                {
                    // num verts
                    Array.Copy(
                        BitConverter.GetBytes(mesh.NumVerts), 0,
                        buffer, index,
                        4);
                    index += 4;

                    Log(mesh.NumVerts);

                    // num tris
                    Array.Copy(
                        BitConverter.GetBytes(mesh.NumTris), 0,
                        buffer, index,
                        4);
                    index += 4;

                    Log(mesh.NumTris);
                }

                // pack verts
                {
                    var localToWorld = mesh.LocalToWorld;
                    var verts = mesh.Vertices;
                    for (int j = 0, jlen = mesh.NumVerts; j < jlen; j++)
                    {
                        var vert = localToWorld.MultiplyPoint3x4(verts[j]);
                        var x = BitConverter.GetBytes(vert.x);
                        var y = BitConverter.GetBytes(vert.y);
                        var z = BitConverter.GetBytes(vert.z);

                        Array.Copy(x, 0, buffer, index, 4); index += 4;
                        Array.Copy(y, 0, buffer, index, 4); index += 4;
                        Array.Copy(z, 0, buffer, index, 4); index += 4;
                    }
                }

                // pack tris
                {
                    var triangles = mesh.Triangles;
                    for (int j = 0, jlen = mesh.NumTris; j < jlen; j++)
                    {
                        var x = BitConverter.GetBytes((ushort) triangles[j * 3]);
                        var y = BitConverter.GetBytes((ushort) triangles[j * 3 + 1]);
                        var z = BitConverter.GetBytes((ushort) triangles[j * 3 + 2]);

                        Array.Copy(x, 0, buffer, index, 2); index += 2;
                        Array.Copy(y, 0, buffer, index, 2); index += 2;
                        Array.Copy(z, 0, buffer, index, 2); index += 2;
                    }
                }
            }

            Assert.AreEqual(index, buffer.Length, "Index mismatch.");
            
            return buffer;
        }

        private static void Log(object value)
        {

        }
    }
}