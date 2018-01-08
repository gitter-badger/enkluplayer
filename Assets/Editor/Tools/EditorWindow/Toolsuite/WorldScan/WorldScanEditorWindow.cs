using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    public class TableComponent : IEditorView
    {
        private class TableColumnData
        {
            public readonly FieldInfo Field;
            public float Width;

            public TableColumnData(FieldInfo field)
            {
                Field = field;
            }
        }

        private TableColumnData[] _columns;
        private object[] _elements;

        public object[] Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                Unpopulate();

                _elements = value;

                Populate();
            }
        }

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            GUILayout.BeginVertical();
            {
                DrawHeaders();
                DrawRows();
            }
            GUILayout.EndVertical();
        }

        private void DrawRows()
        {
            if (null == _elements)
            {
                return;
            }

            foreach (var row in _elements)
            {
                GUILayout.BeginHorizontal();
                {
                    foreach (var column in _columns)
                    {
                        try
                        {
                            var value = column.Field.GetValue(row);
                            GUILayout.Label(
                                null == value
                                    ? "No value"
                                    : value.ToString(),
                                GUILayout.Width(column.Width));
                        }
                        catch (Exception exception)
                        {
                            Debug.Log("Could not GetValue : " + exception.Message);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawHeaders()
        {
            GUILayout.BeginHorizontal();
            {
                foreach (var column in _columns)
                {
                    GUILayout.Label(column.Field.Name);

                    if (Event.current.type == EventType.Repaint)
                    {
                        column.Width = GUILayoutUtility.GetLastRect().width;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void Unpopulate()
        {
            _columns = new TableColumnData[0];
        }

        private void Populate()
        {
            if (null == _elements)
            {
                return;
            }

            var columns = new HashSet<FieldInfo>();
            foreach (var element in _elements)
            {
                var fields = element.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (var field in fields)
                {
                    columns.Add(field);
                }
            }

            _columns = columns
                .Select(field => new TableColumnData(field))
                .ToArray();
        }
    }

    public class WorldScanView : IEditorView
    {
        public class WorldScanRecord
        {
            public string Name;
            public string Uri;
            public string LastUpdated;
            public string Tags;
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