using System.IO;
using UnityEditor;
using UnityEngine;
using EditorApplication = CreateAR.SpirePlayer.Editor.EditorApplication;

namespace CreateAR.SpirePlayer.Test
{
    /// <summary>
    /// Runs an obj exporter.
    /// </summary>
    public static class MeshCommands
    {
        /// <summary>
        /// Exports game objects as obj.
        /// </summary>
        [MenuItem("Tools/Mesh/Export Meshes")]
        private static void Export()
        {
            var path = EditorUtility.SaveFilePanel("Save", ".", "Export", "mesh");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            File.WriteAllBytes(
                path,
                new MeshExporter().Export(Selection.gameObjects));

            EditorUtility.DisplayDialog(
                "Export",
                string.Format("Mesh exported to {0}.", path),
                "Ok");
        }

        /// <summary>
        /// Exports game objects as obj.
        /// </summary>
        [MenuItem("Tools/Mesh/Import Meshes")]
        private static void Import()
        {
            if (!EditorApplication.IsRunning)
            {
                EditorUtility.DisplayDialog(
                    "Whoops!",
                    "Editor cannot be in playmode when importing a mesh.",
                    "Okay");
                return;
            }

            var path = EditorUtility.OpenFilePanel("Open", ".", "mesh");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var bytes = File.ReadAllBytes(path);
            EditorApplication.ScanImporter.Import(
                bytes,
                (exception, execute) =>
                {
                    execute(new GameObject());

                    EditorUtility.DisplayDialog(
                        "Import",
                        "Mesh imported.",
                        "Ok");
                });
        }
    }
}