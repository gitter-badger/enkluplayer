﻿using System.Collections.Generic;
using Enklu.Orchid;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    [JsInterface("drawing")]
    [JsDeclaredOnly]
    public class DrawingJsApi : MonoBehaviour
    {
        /// <summary>
        /// Record for callback.
        /// </summary>
        private class CallbackRecord
        {
            /// <summary>
            /// The associated category that can be filtered on.
            /// </summary>
            public string Category;

            /// <summary>
            /// The callback to call.
            /// </summary>
            public IJsCallback Callback;

            /// <summary>
            /// The execution context used to execute the callback
            /// </summary>
            public IJsExecutionContext ExecutionContext;
        }

        /// <summary>
        /// List of callbacks received.
        /// </summary>
        private readonly List<CallbackRecord> _cbs = new List<CallbackRecord>();

        /// <summary>
        /// Does the drawing.
        /// </summary>
        private readonly ContextJsApi _context = new ContextJsApi();

        /// <summary>
        /// Filters renderer contexts.
        /// </summary>
        private string _filter = string.Empty;

        /// <summary>
        /// The material to draw with, programmatically generated.
        /// </summary>
        private Material _material;

        /// <summary>
        /// Filters callbacks.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void filter(string filter)
        {
            _filter = (filter ?? string.Empty).ToLower();
        }

        /// <summary>
        /// Registers a callback for drawing.
        /// </summary>
        /// <param name="category">The category associated with this callback.</param>
        /// <param name="fn">The callback.</param>
        public void register(string category, IJsCallback fn)
        {
            // Execution Context used in callback
            var executionContext = fn.ExecutionContext;

            // this may be called multiple times on the same engine
            executionContext.OnExecutionContextDisposing -= OnExecutionContextDisposing;
            executionContext.OnExecutionContextDisposing += OnExecutionContextDisposing;

            _cbs.Add(new CallbackRecord
            {
                Category = category.ToLower(),
                Callback = fn,
                ExecutionContext = executionContext
            });
        }

        /// <summary>
        /// Removes a callback for drawing.
        /// </summary>
        /// <param name="fn">The callback to remove.</param>
        public void unregister(IJsCallback fn)
        {
            for (int i = 0, len = _cbs.Count; i < len; i++)
            {
                if (fn == _cbs[i].Callback)
                {
                    _cbs.RemoveAt(i);
                    return;
                }
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            _material = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _material.SetInt("_ZWrite", 0);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnPostRender()
        {
            var len = _cbs.Count;
            if (len > 0)
            {
                _material.SetPass(0);

                for (var i = 0; i < len; i++)
                {
                    var record = _cbs[i];
                    if (!record.Category.StartsWith(_filter))
                    {
                        continue;
                    }

                    _context.ResetState();

                    try
                    {
                        record.Callback.Apply(this, _context);
                    }
                    catch
                    {
                        //
                    }
                }
            }
        }

        /// <summary>
        /// Called when an execution context has been destroyed.
        /// </summary>
        /// <param name="engine"></param>
        private void OnExecutionContextDisposing(IJsExecutionContext jsExecutionContext)
        {
            jsExecutionContext.OnExecutionContextDisposing -= OnExecutionContextDisposing;

            // remove all callbacks related to this engine
            for (var i = _cbs.Count - 1; i >= 0; i--)
            {
                if (_cbs[i].ExecutionContext == jsExecutionContext)
                {
                    _cbs.RemoveAt(i);
                }
            }
        }
    }
}
