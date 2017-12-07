using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.UI;
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

                    // TODO: Remove
                    if (null == element)
                    {
                        // find widgetmonobehaviour
                        var behaviour = value.GetComponent<WidgetMonoBehaviour>();
                        if (null != behaviour)
                        {
                            element = behaviour.Widget;
                        }
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
            foreach (var prop in _selection.Schema)
            {
                var type = prop.Type;

                PropRenderer renderer;
                if (_propRenderers.TryGetValue(type, out renderer))
                {
                    repaint = renderer.Draw(prop) || repaint;
                }
            }

            if (repaint)
            {
                Repaint();
            }
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