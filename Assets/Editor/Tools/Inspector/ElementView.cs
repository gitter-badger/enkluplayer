using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Editor;
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
        /// The currently selected Widget.
        /// </summary>
        private Widget _selection;

        /// <summary>
        /// Current GameObject selected.
        /// </summary>
        public GameObject Selection
        {
            set
            {
                Widget widget = null;

                if (value == null)
                {
                    if (_selection == null)
                    {
                        return;
                    }

                    _selection = null;
                }
                else
                {
                    var behaviour = value.GetComponent<WidgetMonoBehaviour>();
                    if (null != behaviour)
                    {
                        widget = behaviour.Widget;
                    }

                    if (widget == _selection)
                    {
                        return;
                    }
                }

                _selection = widget;

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
    }
}