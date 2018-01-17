﻿using System.IO;
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

            using (var stream = File.OpenWrite(path))
            {
                var bytes = new MeshExporter().Export(Selection.gameObjects);

                stream.Write(bytes, 0, bytes.Length);
            }
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
            EditorApplication.MeshImporter.Import(
                bytes,
                execute =>
                {
                    execute(new GameObject());
                });
        }
    }
}