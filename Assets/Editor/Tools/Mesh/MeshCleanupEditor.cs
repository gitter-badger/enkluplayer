using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Cleans up meshes.
    /// </summary>
    public class MeshCleanupEditor : EditorWindow
    {
        /// <summary>
        /// Current object to clean.
        /// </summary>
        private GameObject _instance;
        
        /// <summary>
        /// Max length for a triangle side.
        /// </summary>
        private float _maxLen = 100f;

        /// <summary>
        /// Meshes to process.
        /// </summary>
        private Mesh[] _meshes = new Mesh[0];
        private MeshFilter[] _meshFilters = new MeshFilter[0];

        /// <summary>
        /// Original triangles of the mesh.
        /// </summary>
        private int[][] _originalTriangles = new int[0][];

        /// <summary>
        /// Log messages.
        /// </summary>
        private string _log;

        /// <summary>
        /// Opens the tool.
        /// </summary>
        [MenuItem("Tools/Mesh/Cleanup")]
        private static void Open()
        {
            GetWindow<MeshCleanupEditor>();
        }

        /// <summary>
        /// Called on enable.
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Cleanup");
        }

        /// <summary>
        /// Called when disabled.
        /// </summary>
        private void OnDisable()
        {
            _instance = null;
            _meshes = new Mesh[0];
            _meshFilters = new MeshFilter[0];
            _originalTriangles = new int[0][];
        }

        /// <summary>
        /// Called to draw controls.
        /// </summary>
        private void OnGUI()
        {
            var gameObject = Selection.activeGameObject;

            if (null == gameObject)
            {
                GUILayout.Label("Select a GameObject to continue.");

                _instance = null;
                _meshes = new Mesh[0];
                _meshFilters = new MeshFilter[0];
                _originalTriangles = new int[0][];

                return;
            }

            if (gameObject != _instance)
            {
                _instance = gameObject;

                _meshFilters = _instance
                    .GetComponentsInChildren<MeshFilter>()
                    .ToArray();
                _meshes = _meshFilters
                    .Select(filter =>
                    {
                        var mesh = filter.sharedMesh;

                        Undo.RecordObject(_instance, "Mark Dynamic");

                        mesh.MarkDynamic();
                        return mesh;
                    })
                    .ToArray();
                _originalTriangles = new int[_meshes.Length][];
                for (int i = 0, len = _meshes.Length; i < len; i++)
                {
                    _originalTriangles[i] = _meshes[i].triangles;
                }
            }

            _maxLen = EditorGUILayout.FloatField("Max Triangle Size", _maxLen);
            
            if (GUILayout.Button("Revert"))
            {
                RevertMeshes();
            }

            if (GUILayout.Button("Save"))
            {
                SaveMeshes();
            }

            ProcessMeshes();

            GUI.enabled = false;
            GUILayout.TextArea(_log ?? "No actions have been performed.");
            GUI.enabled = true;
        }

        /// <summary>
        /// Processes the mesh geometry.
        /// </summary>
        private void ProcessMeshes()
        {
            _log = string.Format("Processing {0} meshes.", _meshes.Length);

            for (int i = 0, len = _meshes.Length; i < len; i++)
            {
                var tris = _originalTriangles[i];

                ProcessMesh(_meshes[i], ref tris);
            }
        }

        /// <summary>
        /// Processes a single mesh.
        /// </summary>
        /// <param name="mesh">The mesh to process.</param>
        /// <param name="triangles">The mesh's original triangles.</param>
        private void ProcessMesh(Mesh mesh, ref int[] triangles)
        {
            var verts = mesh.vertices;
            var newTriangles = new List<int>();
            var len = triangles.Length;

            var lensq = _maxLen * _maxLen;
            for (var i = 0; i < len;)
            {
                var ia = triangles[i++];
                var ib = triangles[i++];
                var ic = triangles[i++];

                var a = verts[ia];
                var b = verts[ib];
                var c = verts[ic];

                if ((a - b).sqrMagnitude < lensq
                    && (b - c).sqrMagnitude < lensq
                    && (c - a).sqrMagnitude < lensq)
                {
                    newTriangles.Add(ia);
                    newTriangles.Add(ib);
                    newTriangles.Add(ic);
                }
            }
            
            mesh.triangles = newTriangles.ToArray();

            _log += string.Format("{0} triangles -> {1} triangles ({2})\n",
                len,
                newTriangles.Count,
                mesh.triangles.Length);
        }

        /// <summary>
        /// Reverts the mesh to its initial triangles.
        /// </summary>
        private void RevertMeshes()
        {
            _maxLen = 100f;

            for (int i = 0, len = _meshes.Length; i < len; i++)
            {
                _meshes[i].triangles = _originalTriangles[i];
            }
        }

        /// <summary>
        /// Saves mesh as obj.
        /// </summary>
        private void SaveMeshes()
        {
            // create duplicate meshes
            var root = new GameObject("Export");
            for (var i = 0; i < _meshes.Length; i++)
            {
                var mesh = _meshes[i];
                var handle = CreateDuplicate(mesh, i);
                handle.transform.SetParent(root.transform, true);
            }

            AssetDatabase.SaveAssets();
            PrefabUtility.CreatePrefab("Assets/Exports/Export.prefab", root);

            var export = new MeshExporter().Export(root);
            File.WriteAllBytes("Assets/Exports/Exported_obj.obj", export);

            DestroyImmediate(root);
        }

        /// <summary>
        /// Creates a duplicate of a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to duplicate.</param>
        /// <param name="index">Index.</param>
        /// <returns></returns>
        private GameObject CreateDuplicate(Mesh mesh, int index)
        {
            var newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.normals = mesh.normals;
            newMesh.uv = mesh.uv;
            newMesh.triangles = mesh.triangles;

            Directory.CreateDirectory("Assets/Exports");

            AssetDatabase.CreateAsset(newMesh,
                string.Format("Assets/Exports/Exported_{0}.asset", index));

            var gameObject = new GameObject("Mesh");
            gameObject.AddComponent<MeshFilter>().sharedMesh = newMesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

            return gameObject;
        }
    }
}