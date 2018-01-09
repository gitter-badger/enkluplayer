using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    public class HttpControllerView : IEditorView
    {
        private static readonly string[] IGNORED_METHODS = new[]
        {
            "GetHashCode",
            "ToString",
            "GetType",
            "Equals"
        };

        private readonly Dictionary<Type, FormInspector> _forms = new Dictionary<Type, FormInspector>();
        private object _selectedController = null;
        private MethodInfo _selectedMethod = null;

        public event Action OnRepaintRequested;
        
        public void Draw()
        {
            GUILayout.BeginVertical();
            {
                var api = EditorApplication.Api;

                // each controller
                var controllers = api
                    .GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Select(field => field.GetValue(api));
                foreach (var controller in controllers)
                {
                    DrawController(controller);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawController(object controller)
        {
            var selected = EditorGUILayout.Foldout(
                controller == _selectedController,
                controller.GetType().Name);
            if (selected)
            {
                _selectedController = controller;

                var methods = controller
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(method => !IGNORED_METHODS.Contains(method.Name));
                foreach (var method in methods)
                {
                    DrawMethod(controller, method);
                }
            }
            else if (controller == _selectedController)
            {
                _selectedController = null;
            }
        }

        private void DrawMethod(object controller, MethodInfo method)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(20);

                var selected = EditorGUILayout.Foldout(
                    method == _selectedMethod,
                    method.Name);
                if (selected)
                {
                    _selectedMethod = method;

                    DrawForm(controller, method);
                }
                else if (_selectedMethod == method)
                {
                    _selectedMethod = null;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawForm(object controller, MethodInfo method)
        {
            var parameters = method.GetParameters();

            GUILayout.BeginVertical("box");
            {
                foreach (var param in parameters)
                {
                    if (param.GetType().IsPrimitive)
                    {

                    }
                    else
                    {
                        FormInspector form;
                        if (!_forms.TryGetValue(param.ParameterType, out form))
                        {
                            form = _forms[param.ParameterType] = new FormInspector();
                            form.Value = Activator.CreateInstance(param.ParameterType);
                        }

                        form.Draw();
                    }
                }
            }
            GUILayout.EndVertical();

            if (GUILayout.Button("Send"))
            {
                
            }
        }

        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }
    }
}