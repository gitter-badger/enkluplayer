using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Easy context menu wins.
    /// </summary>
    public static class ContextMenuHooks
    {
        /// <summary>
        /// Copies path to clipboard.
        /// </summary>
        [MenuItem("Assets/Copy Path", false, 1)]
        private static void CopyPath()
        {
            Copy(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        /// <summary>
        /// Copies guid to clipboard.
        /// </summary>
        [MenuItem("Assets/Copy Guid", false, 0)]
        private static void CopyGuid()
        {
            Copy(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
        }

        /// <summary>
        /// Generates AssetData XML for all objects in directory + copies to
        /// clipboard.
        /// </summary>
        [MenuItem("Assets/Data/Copy Asset Xml", false, 0)]
        private static void GenerateAssetXml()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var assets = Directory
                .GetFiles(path)
                .Select(AssetDatabase.LoadAssetAtPath<Object>)
                .Where(asset => null != asset)
                .Select(asset => new AssetData
                {
                    AssetName = asset.name,
                    Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)),
                    Uri = AssetDatabase.GetAssetPath(asset)
                })
                .ToArray();

            var serializer = new SystemXmlSerializer();
            byte[] bytes;
            serializer.Serialize(new AssetDataManifest
            {
                Assets = assets
            }, out bytes);

            Copy(Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Generates ScriptData XML for all objects in directory, recursively.
        /// </summary>
        [MenuItem("Assets/Data/Generate ScriptData Xml", false, 0)]
        private static void GenerateScriptDataXml()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            // ScriptData path
            var scriptDataPath = Path.Combine(path, "../ScriptData");

            Action<string> processDir = null;
            processDir = dir =>
            {
                Directory
                    .GetFiles(dir)
                    .Select(AssetDatabase.LoadAssetAtPath<TextAsset>)
                    .Where(asset => null != asset)
                    .Select(asset => new ScriptData
                    {
                        Asset = new AssetReference
                        {
                            AssetDataId = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))
                        },
                        Name = asset.name,
                        Id = asset.name
                    })
                    .ToList()
                    .ForEach(scriptData =>
                    {
                        var documentPath = Path.Combine(scriptDataPath, scriptData.Id + ".local");
                        if (File.Exists(documentPath))
                        {
                            return;
                        }

                        var serializer = new SystemXmlSerializer();
                        byte[] bytes;
                        serializer.Serialize(scriptData, out bytes);

                        File.WriteAllBytes(documentPath, bytes);
                    });

                // recursive
                Directory
                    .GetDirectories(dir)
                    .ToList()
                    .ForEach(processDir);
            };

            processDir(path);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        /// <summary>
        /// Copies text to clipboard.
        /// </summary>
        /// <param name="text">Text to copy.</param>
        private static void Copy(string text)
        {
            var editor = new TextEditor
            {
                text = text
            };

            editor.SelectAll();
            editor.Copy();
        }
    }
}
