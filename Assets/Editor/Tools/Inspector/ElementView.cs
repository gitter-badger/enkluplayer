using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreateAR.Commons.Unity.Editor;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Draws controls for an element.
    /// </summary>
    public class ElementView : IEditorView
    {
        /// <summary>
        /// Lookup from type to the renderer for that type.
        /// </summary>
        private readonly Dictionary<Type, PropRenderer> _propRenderers = new Dictionary<Type, PropRenderer>();

        /// <summary>
        /// Tracks which controls have already been rendered.
        /// </summary>
        private readonly List<string> _renderedProps = new List<string>();

        /// <summary>
        /// Parses element names.
        /// </summary>
        private readonly Regex _elementParser = new Regex(@"Guid=([a-zA-Z0-9\-]+)");

        /// <summary>
        /// Manages elements.
        /// </summary>
        private ElementManager _elements;

        /// <summary>
        /// The currently selected Widget.
        /// </summary>
        private Element _selection;

        /// <summary>
        /// Scroll position.
        /// </summary>
        private Vector2 _position;

        /// <summary>
        /// Current GameObject selected.
        /// </summary>
        public GameObject Selection
        {
            set
            {
                Element element = null;
                
                if (null != value)
                {
                    // parse name
                    var name = value.name;
                    var match = _elementParser.Match(name);
                    if (match.Success)
                    {
                        var id = match.Groups[1].Value;
                        element = FindElement(id);
                    }
                }

                if (element == _selection)
                {
                    return;
                }

                _selection = element;

                Repaint();
            }
        }

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementView()
        {
            TypeUtil.ForAllTypes(type =>
            {
                if (!type.IsSubclassOf(typeof(PropRenderer)))
                {
                    return;
                }

                var attributes = type.GetCustomAttributes(
                    typeof(PropRendererAttribute),
                    true);
                if (0 == attributes.Length)
                {
                    return;
                }

                var controlTypeAttribute = (PropRendererAttribute) attributes[0];
                _propRenderers[controlTypeAttribute.Type] = (PropRenderer) Activator.CreateInstance(type);
            });
        }

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            if (null == _selection)
            {
                return;
            }

            var repaint = false;
            _renderedProps.Clear();

            _position = GUILayout.BeginScrollView(_position);
            {
                var schema = _selection.Schema;
                while (null != schema)
                {
                    repaint = DrawSchema(schema) || repaint;

                    schema = schema.Parent;
                }
            }
            GUILayout.EndScrollView();

            if (repaint)
            {
                Repaint();
            }
        }

        /// <summary>
        /// Draws a schema.
        /// </summary>
        /// <param name="schema">Schema.</param>
        /// <returns></returns>
        private bool DrawSchema(ElementSchema schema)
        {
            var repaint = false;
            GUILayout.BeginVertical("box");
            {
                foreach (var prop in schema)
                {
                    var type = prop.Type;
                    var name = prop.Name;

                    if (_renderedProps.Contains(name))
                    {
                        GUI.enabled = false;
                    }
                    else
                    {
                        GUI.enabled = true;

                        _renderedProps.Add(name);
                    }

                    PropRenderer renderer;
                    if (_propRenderers.TryGetValue(type, out renderer))
                    {
                        repaint = renderer.Draw(prop) || repaint;
                    }
                }
            }
            GUILayout.EndVertical();
            
            return repaint;
        }

        /// <summary>
        /// Calls the repaint event.
        /// </summary>
        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }

        /// <summary>
        /// Find element.
        /// </summary>
        /// <param name="guid">Guid of the element.</param>
        /// <returns></returns>
        private Element FindElement(string guid)
        {
            if (null == _elements)
            {
                _elements = UnityEngine.Object.FindObjectOfType<ElementManager>();
            }

            if (null == _elements)
            {
                return null;
            }

            return _elements.ByGuid(guid);
        }
    }
}