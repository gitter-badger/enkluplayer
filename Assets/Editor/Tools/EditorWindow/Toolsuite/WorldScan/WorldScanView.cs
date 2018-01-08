using System;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// View for world scans.
    /// </summary>
    public class WorldScanView : IEditorView
    {
        /// <summary>
        /// Record for a world scan.
        /// </summary>
        public class WorldScanRecord
        {
            public string Name;
            public Action Download;
        }
        
        /// <summary>
        /// Table to display scans in.
        /// </summary>
        private readonly TableComponent _table = new TableComponent();

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldScanView()
        {

        }

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Refresh"))
                    {
                        Refresh();
                    }
                }
                GUILayout.EndHorizontal();

                _table.Draw();
            }
            GUILayout.EndVertical();
        }

        private void Refresh()
        {
            Log.Info(this, "Requesting my files...");

            EditorApplication
                .Api
                .Files
                .GetMyFiles()
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess && response.Payload.Success)
                    {
                        Log.Info(this, "Found {0} files.", response.Payload.Body.Length);

                        _table.Elements = response.Payload.Body;
                    }
                })
                .OnFailure(exception => Log.Error(this, exception));
        }
    }
}