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
        /// <summary>
        /// Ignore these methods on object.
        /// </summary>
        private static readonly string[] IGNORED_METHODS = 
        {
            "GetHashCode",
            "ToString",
            "GetType",
            "Equals"
        };

        /// <summary>
        /// Forms for each method.
        /// </summary>
        private readonly Dictionary<MethodInfo, HttpControllerMethodForm> _methodForms = new Dictionary<MethodInfo, HttpControllerMethodForm>();

        /// <summary>
        /// Which controller is currently selected.
        /// </summary>
        private object _selectedController = null;

        /// <summary>
        /// Which method is currently selected.
        /// </summary>
        private MethodInfo _selectedMethod = null;

        /// <summary>
        /// Current scroll position.
        /// </summary>
        private Vector2 _scrollPosition;

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
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
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws controls for an HttpController.
        /// </summary>
        /// <param name="controller">The HttpController.</param>
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

        /// <summary>
        /// Draws controls for a specific method.
        /// </summary>
        /// <param name="controller">The HttpController.</param>
        /// <param name="method">The method.</param>
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

        /// <summary>
        /// Draws a form.
        /// </summary>
        /// <param name="controller">HttpController.</param>
        /// <param name="method">The method to draw a form for.</param>
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

        /// <summary>
        /// Safely calls repaint event.
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