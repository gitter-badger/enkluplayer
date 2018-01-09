using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Creates a form for a method.
    /// </summary>
    public class HttpControllerMethodForm : IEditorView
    {
        /// <summary>
        /// Method the form is being rendered for.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Controller.
        /// </summary>
        public object Controller { get; private set; }

        /// <summary>
        /// Parameters of method.
        /// </summary>
        private readonly ParameterInfo[] _parameters;

        /// <summary>
        /// State of parameters.
        /// </summary>
        private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

        /// <summary>
        /// Drawing methods to call in succession.
        /// </summary>
        private readonly List<Action> _drawingMethods = new List<Action>();

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpControllerMethodForm(MethodInfo method, object controller)
        {
            Method = method;
            Controller = controller;

            _parameters = method.GetParameters();

            for (int i = 0, len = _parameters.Length; i < len; i++)
            {
                var parameter = _parameters[i];
                var type = parameter.ParameterType;
                if (type == typeof(string))
                {
                    _state[parameter.Name] = string.Empty;

                    _drawingMethods.Add(() =>
                    {
                        _state[parameter.Name] = EditorGUILayout.TextField(
                            parameter.Name,
                            (string) _state[parameter.Name]);
                    });
                }
                else if (type.IsPrimitive)
                {
                    // TODO
                }
                else
                {
                    var value = _state[parameter.Name] = Activator.CreateInstance(type);
                    var form = new FormInspector
                    {
                        Value = value
                    };
                    form.OnRepaintRequested += Repaint;

                    _drawingMethods.Add(form.Draw);
                }
            }
        }

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            if (_drawingMethods.Count > 0)
            {
                GUILayout.BeginVertical("box");
                {
                    for (int i = 0, len = _drawingMethods.Count; i < len; i++)
                    {
                        _drawingMethods[i]();
                    }
                }
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Send"))
            {
                var arguments = _parameters
                    .Select(parameter => _state[parameter.Name])
                    .ToArray();
                Method.Invoke(Controller, arguments);
            }
        }

        /// <summary>
        /// Called repaint event safely.
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