using System;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    public class WorldScanView : IEditorView
    {
        public class WorldScanRecord
        {
            public string Name;
            public Action Download;
        }
        
        private readonly TableComponent _table = new TableComponent();

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        public WorldScanView()
        {
            _table.Elements = new object[]
            {
                new WorldScanRecord
                {
                    Name = "Scan A",
                    Download = WorldScanRecord_Clicked
                },
                new WorldScanRecord
                {
                    Name = "Scan B"
                }
            };
        }

        private void WorldScanRecord_Clicked()
        {
            Log.Info(this, "Clicked!");
        }

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
            {
                _table.Draw();
            }
            GUILayout.EndVertical();
        }
    }

    public class WorldScanEditorWindow : EditorWindow
    {
        private readonly WorldScanView _view = new WorldScanView();

        [MenuItem("Tools/World Scans")]
        private static void Open()
        {
            GetWindow<WorldScanEditorWindow>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("World Scans");

            _view.OnRepaintRequested += Repaint;
        }

        private void OnDisable()
        {
            _view.OnRepaintRequested -= Repaint;
        }

        private void OnGUI()
        {
            _view.Draw();
        }
    }
}