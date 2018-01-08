using System;
using CreateAR.Commons.Unity.Editor;
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
            _table.Elements = new object[]
            {
                new WorldScanRecord
                {
                    Name = "Scan A"
                },
                new WorldScanRecord
                {
                    Name = "Scan B"
                }
            };
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
}