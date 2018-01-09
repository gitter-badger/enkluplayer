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
        private readonly Dictionary<Type, FormInspector> _forms = new Dictionary<Type, FormInspector>();
        private object _selectedController = null;
        private MethodInfo _selectedMethod = null;

        public event Action OnRepaintRequested;
        
        public void Draw()
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
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var method in methods)
                {
                    DrawMethod(controller, method);
                }
            }
        }

        private void DrawMethod(object controller, MethodInfo method)
        {
            var selected = EditorGUILayout.Foldout(
                method == _selectedMethod,
                method.Name);
            if (selected)
            {
                _selectedMethod = method;

                DrawForm(controller, method);
            }
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