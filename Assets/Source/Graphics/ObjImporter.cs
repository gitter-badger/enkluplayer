using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Imports Objs at runtime in a threadpool.
    /// </summary>
    public class ObjImporter
    {
        /// <summary>
        /// Internal state.
        /// </summary>
        private class ObjImportState
        {
            /// <summary>
            /// Obj source.
            /// </summary>
            public string Obj;

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
        public ObjImporter(IBootstrapper bootstrapper)
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
        /// Imports an obj from source text. The callback is passed a function
        /// that will construct the meshes on a GameObject. This allows the 
        /// caller of this method to decide when to actually create the meshes.
        /// </summary>
        /// <param name="obj">The obj source text.</param>
        /// <param name="callback">The callback.</param>
        public void Import(
            string obj,
            Action<Action<GameObject>> callback)
        {
            var state = new ObjImportState
            {
                Obj = obj,
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
            var meshes = OBJLoader.LoadOBJFile(importState.Obj);

            lock (_synchronizedActions)
            {
                // add an action to be run on the main thread
                _synchronizedActions.Add(() =>
                {
                    importState.Callback(gameObject =>
                    {
                        // APPLY
                        foreach (var mesh in meshes)
                        {
                            var child = new GameObject(mesh.Name);
                            child.transform.parent = gameObject.transform;
                            child.transform.localScale = new Vector3(-1, 1, 1);
                            child.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard (Specular setup)"));

                            ApplyMesh(child, mesh);
                        }
                    });
                });
            }
        }
        
        /// <summary>
        /// Applies the MeshInfo to a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <param name="info">The mesh information.</param>
        private static void ApplyMesh(GameObject gameObject, OBJLoader.MeshInfo info)
        {
            var mesh = new Mesh
            {
                vertices = info.Vertices,
                normals = info.Normals,
                uv = info.Uvs,
                subMeshCount = info.SubMeshCount
            };

            for (var i = 0; i < info.Triangles.Length; i++)
            {
                mesh.SetTriangles(info.Triangles[i], i);
            }

            if (!info.HasNormals)
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();

            gameObject.AddComponent<MeshFilter>().mesh = mesh;
        }
    }
}