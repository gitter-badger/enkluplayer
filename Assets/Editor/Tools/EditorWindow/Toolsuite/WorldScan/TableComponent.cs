
using System;
using System.Collections.Generic;
using System.Reflection;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Component for tabular data.
    /// </summary>
    public class TableComponent : IEditorView
    {
        /// <summary>
        /// Stores column information.
        /// </summary>
        private class TableColumnData
        {
            /// <summary>
            /// Label for this column.
            /// </summary>
            public readonly string Label;

            /// <summary>
            /// Name of the field for this column.
            /// </summary>
            public readonly string FieldName;

            /// <summary>
            /// Type of field.
            /// </summary>
            public readonly Type FieldType;

            /// <summary>
            /// Column width.
            /// </summary>
            public float Width;

            /// <summary>
            /// Constructor.
            /// </summary>
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

        /// <summary>
        /// Caches field information.
        /// </summary>
        private class FieldInfoCache
        {
            /// <summary>
            /// Fields.
            /// </summary>
            public readonly FieldInfo[] Fields;

            /// <summary>
            /// Constructor.
            /// </summary>
            public FieldInfoCache(Type type)
            {
                Fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            }

            /// <summary>
            /// Retrieves a matching value and returns true iff the field
            /// exists.
            /// </summary>
            /// <param name="fieldName">Name of the field.</param>
            /// <param name="fieldType">Type of field.</param>
            /// <param name="instance">Instance to pull value off of.</param>
            /// <param name="value">Output value.</param>
            /// <returns>True iff field exists on object.</returns>
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

        /// <summary>
        /// Caches by type.
        /// </summary>
        private static readonly Dictionary<Type, FieldInfoCache> _caches = new Dictionary<Type, FieldInfoCache>();

        /// <summary>
        /// Columns, populated on Populate();
        /// </summary>
        private TableColumnData[] _columns = new TableColumnData[0];

        /// <summary>
        /// Backing variable for Elements property.
        /// </summary>
        private object[] _elements = new object[0];

        /// <summary>
        /// Elements to display information for.
        /// </summary>
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
                EditorGUILayout.Separator();
                DrawRows();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws column headers.
        /// </summary>
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

        /// <summary>
        /// Draws all column rows.
        /// </summary>
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
                            if (column.FieldType == typeof(Action))
                            {
                                if (GUILayout.Button(
                                    column.FieldName,
                                    GUILayout.Width(column.Width))
                                && null != value)
                                {
                                    ((Action) value)();

                                    Repaint();
                                }
                            }
                            else
                            {
                                GUILayout.Label(
                                    null == value
                                        ? (string)GetDefault(column.FieldType)
                                        : value.ToString(),
                                    GUILayout.Width(column.Width));
                            }
                        }
                        else
                        {
                            GUILayout.Label(
                                "--",
                                GUILayout.Width(column.Width));
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Unpopulates data.
        /// </summary>
        private void Unpopulate()
        {
            _columns = new TableColumnData[0];
            _elements = new object[0];
        }

        /// <summary>
        /// Populates data.
        /// </summary>
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

        /// <summary>
        /// Retrieves default value for type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns></returns>
        private object GetDefault(Type type)
        {
            return GetType()
                .GetMethod("GetDefaultGeneric", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type)
                .Invoke(this, null);
        }

        /// <summary>
        /// Retrieves default value for type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns></returns>
        private T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Requests a repaint.
        /// </summary>
        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }
    }
}