using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Creates a form for a method.
    /// </summary>
    public class HttpControllerMethodForm : IEditorView
    {
        /// <summary>
        /// Keeps track of last response.
        /// </summary>
        private class ResponseRecord
        {
            /// <summary>
            /// Repaint action.
            /// </summary>
            private readonly Action _onRepaint;

            /// <summary>
            /// Backing variable for prop.
            /// </summary>
            private Action _draw;

            /// <summary>
            /// Associated method.
            /// </summary>
            public readonly MethodInfo Method;

            /// <summary>
            /// Constructor.
            /// </summary>
            public ResponseRecord(
                MethodInfo method,
                Action onRepaint)
            {
                Method = method;
                _onRepaint = onRepaint;
            }

            /// <summary>
            /// Action to call to draw stufff.
            /// </summary>
            public Action Draw
            {
                get { return _draw; }
                set
                {
                    _draw = value;

                    _onRepaint();
                }
            }
        }

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

        /// <summary>
        /// Response, if any.
        /// </summary>
        private static ResponseRecord _response;

        /// <summary>
        /// Method the form is being rendered for.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Controller.
        /// </summary>
        public object Controller { get; private set; }
        
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

                    if (null != _response
                        && _response.Method == Method
                        && null != _response.Draw)
                    {
                        _response.Draw();
                    }
                }
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Send"))
            {
                _response = new ResponseRecord(Method, Repaint);

                var arguments = _parameters
                    .Select(parameter => _state[parameter.Name])
                    .ToArray();
                var token = Method.Invoke(Controller, arguments);
                var tokenParameterType = token.GetType().GetGenericArguments()[0];
                var handler = GetType()
                    .GetMethod(
                        "OnResponse",
                        BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(tokenParameterType);
                var actionT = typeof(Action<>).MakeGenericType(tokenParameterType);
                var @delegate = Delegate.CreateDelegate(actionT, handler);
                var onSuccess = token.GetType().GetMethod(
                    "OnSuccess",
                    BindingFlags.Instance | BindingFlags.Public);
                
                onSuccess.Invoke(token, new object[] { @delegate });
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

        /// <summary>
        /// Called when a response comes back.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        private static void OnResponse<T>(T response)
        {
            var rawField = response
                .GetType()
                .GetField("Raw", BindingFlags.Instance | BindingFlags.Public);
            var bytes = (byte[]) rawField.GetValue(response);
            var json = null == bytes
                ? "No response."
                : Encoding.UTF8.GetString(bytes);

            _response.Draw = () =>
            {
                GUILayout.BeginVertical("box");
                {
                    var enabled = GUI.enabled;
                    GUI.enabled = false;
                    GUILayout.TextArea(json);
                    GUI.enabled = enabled;
                }
                GUILayout.EndVertical();
            };
        }
    }
}