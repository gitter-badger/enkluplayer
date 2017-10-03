using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreateAR.SpirePlayer;
using UnityEditor;
using UnityEngine;

namespace CreateAR.Spire.Editor
{
    public class AssetManifestEditor : EditorWindow
    {
        private readonly List<SpirePlayer.AssetData> _infos = new List<SpirePlayer.AssetData>();

        private Vector2 _scrollPosition;

        [MenuItem("Tools/Asset Manifest Editor")]
        private static void Open()
        {
            GetWindow<AssetManifestEditor>();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("Save"))
                {
                    Save();
                }

                if (GUILayout.Button("Add"))
                {
                    _infos.Add(new SpirePlayer.AssetData());
                }

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                {
                    foreach (var info in _infos)
                    {
                        DrawAssetInfo(info);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetInfo(SpirePlayer.AssetData data)
        {
            EditorGUILayout.BeginVertical("box");
            {
                var path = AssetDatabase.GUIDToAssetPath(data.Guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                var selectedAsset = (GameObject) EditorGUILayout.ObjectField(
                    new GUIContent("Asset"),
                    asset,
                    typeof(GameObject),
                    false);

                if (asset != selectedAsset)
                {
                    path = AssetDatabase.GetAssetPath(selectedAsset);
                    data.Guid = AssetDatabase.AssetPathToGUID(path);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void Save()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save",
                UnityEngine.Application.streamingAssetsPath,
                "Assets.local",
                "local");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var manifest = new AssetDataManifest
            {
                Assets = _infos
                    .Select(info =>
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(info.Guid);
                        Debug.Log(string.Format("{0} : {1}", info.Guid, assetPath));
                        info.Uri = assetPath;
                        return info;
                    })
                    .ToArray()
            };

            byte[] bytes;
            var serializer = new SystemXmlSerializer();
            serializer.Serialize(manifest, out bytes);

            File.WriteAllBytes(path, bytes);
        }
    }
}