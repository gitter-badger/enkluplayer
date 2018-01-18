using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Imports meshes at runtime in a threadpool.
    /// </summary>
    public class MeshImporter
    {
        /// <summary>
        /// Internal state.
        /// </summary>
        private class ObjImportState
        {
            /// <summary>
            /// Source buffer.
            /// </summary>
            public byte[] Bytes;

            /// <summary>
            /// Callback that receives a method for construction. This allows
            /// the caller to decide when a GameObject is created.
            /// </summary>
            public Action<Action<GameObject>> Callback;
        }
        
        /// <summary>
        /// "Synchronized" list of actions needed on the main thread.
        /// </summary>
        private static readonly List<Action> _synchronizedActions = new List<Action>();

        /// <summary>
        /// True iff the long-running coroutine should be running.
        /// </summary>
        private bool _isAlive = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshImporter(IBootstrapper bootstrapper)
        {
            bootstrapper.BootstrapCoroutine(Synchronize());
        }

        /// <summary>
        /// Stops the coroutine.
        /// </summary>
        public void Stop()
        {
            _isAlive = false;
        }

        /// <summary>
        /// Imports an obj from source bytes. The callback is passed a function
        /// that will construct the meshes on a GameObject. This allows the 
        /// caller of this method to decide when to actually create the meshes.
        /// </summary>
        /// <param name="bytes">The obj source text.</param>
        /// <param name="callback">The callback.</param>
        public void Import(
            byte[] bytes,
            Action<Action<GameObject>> callback)
        {
            var state = new ObjImportState
            {
                Bytes = bytes,
                Callback = callback
            };

#if NETFX_CORE
            Windows.System.Threading.ThreadPool.RunAsync(_ => Process(state));
#else
            ThreadPool.QueueUserWorkItem(
                Process,
                state);
#endif
        }

        /// <summary>
        /// Long-running coroutine that pulls actions onto the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Synchronize()
        {
            while (_isAlive)
            {
                Action[] copy;
                lock (_synchronizedActions)
                {
                    copy = _synchronizedActions.ToArray();
                    _synchronizedActions.Clear();
                }

                foreach (var action in copy)
                {
                    action();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Static method that processes the Obj source in a threadpool.
        /// </summary>
        /// <param name="state">The obj source.</param>
        private static void Process(object state)
        {
            var importState = (ObjImportState) state;
            var collection = ImportBytes(importState.Bytes);

            lock (_synchronizedActions)
            {
                // add an action to be run on the main thread
                _synchronizedActions.Add(() =>
                {
                    importState.Callback(gameObject =>
                    {
                        // APPLY
                        foreach (var mesh in collection.Meshes)
                        {
                            var child = new GameObject();
                            child.transform.parent = gameObject.transform;
                            child.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard (Specular setup)"));

                            ApplyMesh(child, mesh);
                        }
                    });
                });
            }
        }

        /// <summary>
        /// Imports bytes into a collection of <c>MeshState</c> objects.
        /// </summary>
        /// <param name="bytes">The bytes to import.</param>
        /// <returns></returns>
        private static MeshStateCollection ImportBytes(byte[] bytes)
        {
            var collection = new MeshStateCollection();

            var index = 0;
            while (index < bytes.Length)
            {
                var state = new MeshStateCollection.MeshState();
                collection.Meshes.Add(state);

                int numVerts, numTris;
                
                // read header
                {
                    // num verts
                    numVerts = BitConverter.ToInt32(bytes, index); index += 4;

                    // num tris
                    numTris = BitConverter.ToInt32(bytes, index); index += 4;
                }

                // read verts
                {
                    state.Vertices = new Vector3[numVerts];

                    for (var i = 0; i < numVerts; i++)
                    {
                        state.Vertices[i] = new Vector3(
                            BitConverter.ToSingle(bytes, index),
                            BitConverter.ToSingle(bytes, index + 4),
                            BitConverter.ToSingle(bytes, index + 8));

                        index += 12;
                    }
                }

                // read triangles
                {
                    state.Triangles = new int[numTris * 3];

                    for (var i = 0; i < numTris; i++)
                    {
                        state.Triangles[i * 3] = BitConverter.ToUInt16(bytes, index);
                        index += 2;

                        state.Triangles[i * 3 + 1] = BitConverter.ToUInt16(bytes, index);
                        index += 2;

                        state.Triangles[i * 3 + 2] = BitConverter.ToUInt16(bytes, index);
                        index += 2;
                    }
                }
            }
            
            return collection;
        }

        /// <summary>
        /// Applies the MeshInfo to a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <param name="info">The mesh information.</param>
        private static void ApplyMesh(
            GameObject gameObject,
            MeshStateCollection.MeshState info)
        {
            var mesh = new Mesh();
            mesh.vertices = info.Vertices;
            mesh.triangles = info.Triangles;
            mesh.UploadMeshData(false);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            gameObject.AddComponent<MeshFilter>().mesh = mesh;
        }
    }
}