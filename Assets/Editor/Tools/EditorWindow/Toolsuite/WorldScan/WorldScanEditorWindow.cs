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
            public readonly string Label;
            public readonly string FieldName;
            public readonly Type FieldType;

            public float Width;

            public TableColumnData(
                string label,
                string fieldName,
                Type fieldType)
            {
                Label = label;
                FieldName = fieldName;
                FieldType = fieldType;
            }
        }

        private class FieldInfoCache
        {
            public readonly Type Type;
            public readonly FieldInfo[] Fields;

            public FieldInfoCache(Type type)
            {
                Type = type;
                Fields = Type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            }

            public bool Value(
                string fieldName,
                Type fieldType,
                object instance,
                out object value)
            {
                for (int i = 0, len = Fields.Length; i < len; i++)
                {
                    var field = Fields[i];
                    if (field.Name == fieldName && field.FieldType == fieldType)
                    {
                        value = field.GetValue(instance);
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }

        private static readonly Dictionary<Type, FieldInfoCache> _caches = new Dictionary<Type, FieldInfoCache>();

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

        private void DrawHeaders()
        {
            GUILayout.BeginHorizontal();
            {
                foreach (var column in _columns)
                {
                    GUILayout.Label(column.Label);

                    if (Event.current.type == EventType.Repaint)
                    {
                        column.Width = GUILayoutUtility.GetLastRect().width;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawRows()
        {
            if (null == _elements)
            {
                return;
            }

            foreach (var element in _elements)
            {
                var elementType = element.GetType();
                var cache = _caches[elementType];

                GUILayout.BeginHorizontal();
                {
                    foreach (var column in _columns)
                    {
                        object value;
                        var applicable = cache.Value(
                            column.FieldName,
                            column.FieldType,
                            element,
                            out value);

                        if (applicable)
                        {
                            GUILayout.Label(
                                null == value
                                    ? (string) GetDefault(column.FieldType)
                                    : value.ToString(),
                                GUILayout.Width(column.Width));
                        }
                        else
                        {
                            GUILayout.Label(
                                "N/A",
                                GUILayout.Width(column.Width));
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
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

            var columns = new List<TableColumnData>();
            foreach (var element in _elements)
            {
                var type = element.GetType();

                // make sure cache exists for type
                FieldInfoCache cache;
                if (!_caches.TryGetValue(type, out cache))
                {
                    cache = _caches[type] = new FieldInfoCache(type);
                }

                // add columns
                foreach (var field in cache.Fields)
                {
                    var foundExact = false;
                    var foundNameOnly = false;

                    foreach (var column in columns)
                    {
                        if (column.FieldName == field.Name)
                        {
                            if (column.FieldType == field.FieldType)
                            {
                                foundExact = true;

                                break;
                            }
                            else
                            {
                                foundNameOnly = true;
                            }
                        }
                    }

                    if (foundExact)
                    {
                        continue;
                    }

                    if (foundNameOnly)
                    {
                        columns.Add(new TableColumnData(
                            string.Format("{0} ({1})", field.Name, field.FieldType.Name),
                            field.Name,
                            field.FieldType));
                    }
                    else
                    {
                        columns.Add(new TableColumnData(
                            field.Name,
                            field.Name,
                            field.FieldType));
                    }
                }
            }

            _columns = columns.ToArray();
        }

        private object GetDefault(Type type)
        {
            return GetType()
                .GetMethod("GetDefaultGeneric", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type)
                .Invoke(this, null);
        }

        private T GetDefaultGeneric<T>()
        {
            return default(T);
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
            public string A;
            public float B;
        }

        public class DummyTest
        {
            public string Name;
            public int A;
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
                },
                new DummyTest
                {
                    Name = "Foo",
                    A = 14
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