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
    /// View of all HttpController calls.
    /// </summary>
    public class HttpControllerView : IEditorView
    {
        private static readonly string[] IGNORED_METHODS = new[]
        {
            "GetHashCode",
            "ToString",
            "GetType",
            "Equals"
        };

        private readonly Dictionary<MethodInfo, HttpControllerMethodForm> _methodForms = new Dictionary<MethodInfo, HttpControllerMethodForm>();
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

                GUILayout.BeginVertical();
                {
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
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawForm(object controller, MethodInfo method)
        {
            HttpControllerMethodForm form;
            if (!_methodForms.TryGetValue(method, out form))
            {
                form = _methodForms[method] = new HttpControllerMethodForm(method, controller);
                form.OnRepaintRequested += Repaint;
            }

            form.Draw();
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