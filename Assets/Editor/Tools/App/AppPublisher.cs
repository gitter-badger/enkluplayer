/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreateAR.SpirePlayer;
using UnityEditor;
using UnityEngine;

namespace CreateAR.Spire.Editor
{
    public static class AppPublisher
    {
        private const string EXTENSION = ".htm";

        [MenuItem("Tools/Publish")]
        public static void Publish()
        {
            var appAsset = Selection.activeObject as TextAsset;
            if (null == appAsset)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Select an App.xml in the project view before publishing.",
                    "Ok");
                return;
            }

            var bytes = appAsset.bytes;
            AppData app;
            if (!Deserialize(ref bytes, out app))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Could not deserialize into AppData. Are you sure this is a valid App.xml?\n" + exception.Message,
                    "Ok");
                return;
            }
            
            var path = AssetDatabase.GetAssetPath(appAsset);
            var directory = Path.GetDirectoryName(path);

            // load asset manifest
            bytes = File.ReadAllBytes(Path.Combine(directory, "Assets.local"));
            AssetDataManifest manifest;
            if (!Deserialize(ref bytes, out manifest))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Could not load AssetDataManifest. Are you sure this is a valid app?",
                    "Ok");
                return;
            }

            directory = directory.Replace(UnityEngine.Application.dataPath, "");

            // update manifest
            UpdateManifest(directory, manifest);

            // generate new ScriptData
            GenerateScriptData(directory, manifest);
        }

        private static void GenerateScriptData(
            string parentDirectory,
            AssetDataManifest manifest)
        {
            // get Script paths
            var scriptPaths = Directory
                .GetFiles(Path.Combine(parentDirectory, "Scripts"))
                .Where(file => file.EndsWith(EXTENSION))
                .ToList();

            // delete all ScriptData
            Directory.Delete(Path.Combine(parentDirectory, "ScriptData"), true);
            
            // create new ScriptData
            foreach (var scriptPath in scriptPaths)
            {
                var data = new ScriptData
                {
                    Id = Guid.NewGuid().ToString()
                };
            }

            // update asset manifest
            foreach (var script in scriptIds)
            {
                foreach (var asset in assets)
                {
                    if (asset.Guid == script)
                }
            }
        }

        private static void UpdateManifest(
            string parentDirectory,
            AssetDataManifest manifest)
        {
            var scriptPaths = Directory
                .GetFiles(Path.Combine(parentDirectory, "Scripts"))
                .Where(path => path.EndsWith(EXTENSION))
                .ToList();

            var scriptAssets = scriptPaths
                .Select(AssetDatabase.LoadAssetAtPath<TextAsset>)
                .ToList();

            var assetList = manifest.Assets.ToList();
            for (int i = 0, len = scriptPaths.Count; i < len; i++)
            {
                var scriptPath = scriptPaths[i];
                var scriptAsset = scriptAssets[i];

                var scriptId = Path.GetFileNameWithoutExtension(scriptPath);

                var found = false;
                foreach (var asset in assetList)
                {
                    if (asset.Guid == scriptId)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    assetList.Add(new AssetData
                    {
                        Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptAsset)),
                        Uri = AssetDatabase.GetAssetPath(scriptAsset),
                        AssetName = scriptId
                    });
                }
            }

            // update assets + remove bad assets
            for (var i = assetList.Count - 1; i >= 0; i--)
            {
                var asset = assetList[i];
                var path = AssetDatabase.GUIDToAssetPath(asset.Guid);
                if (string.IsNullOrEmpty(path))
                {
                    assetList.RemoveAt(i);
                }
                else
                {
                    asset.Uri = path;
                }
            }

            manifest.Assets = assetList.ToArray();
        }

        private static bool Deserialize<T>(ref byte[] bytes, out T value)
        {
            var serializer = new SystemXmlSerializer();

            try
            {
                object @object;
                serializer.Deserialize(typeof(T), ref bytes, out @object);

                value = (T) @object;

                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }
    }
}*/