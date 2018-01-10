using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ObjImporter
    {
        private class ObjImportState
        {
            public string Obj;
            public Action<Action<GameObject>> Callback;
        }
        
        private static readonly List<Action> _synchronizedActions = new List<Action>();

        private bool _isAlive = true;

        public ObjImporter(IBootstrapper bootstrapper)
        {
            bootstrapper.BootstrapCoroutine(Synchronize());
        }

        public void Stop()
        {
            _isAlive = false;
        }

        public void Import(
            string obj,
            Action<Action<GameObject>> callback)
        {
            /*ThreadPool.QueueUserWorkItem(
                Process,
                new ObjImportState
                {
                    Obj = obj,
                    Callback = callback
                });*/
            Process(new ObjImportState
            {
                Obj = obj,
                Callback = callback
            });
        }

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

        private static void Process(object state)
        {
            var importState = (ObjImportState) state;
            var meshes = OBJLoader.LoadOBJFile(importState.Obj);

            lock (_synchronizedActions)
            {
                _synchronizedActions.Add(() =>
                {
                    importState.Callback(gameObject =>
                    {
                        /*// APPLY
                        foreach (var mesh in meshes)
                        {
                            var child = new GameObject(mesh.Name);
                            child.AddComponent<MeshRenderer>();
                            child.transform.SetParent(gameObject.transform);

                            ApplyMesh(child, mesh);
                        }*/
                    });
                });
            }
        }
        /*
        private static void ApplyMesh(GameObject gameObject, OBJLoader.MeshInfo info)
        {
            var mesh = new Mesh
            {
                vertices = info.Vertices,
                normals = info.Normals,
                uv = info.Uvs
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
        }*/
    }
}