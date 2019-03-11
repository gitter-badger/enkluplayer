using System.Collections.Generic;
using Enklu.Orchid;
using Jint.Native;
using UnityEngine;

using JsFunc = Enklu.Orchid.IJsCallback;

namespace CreateAR.EnkluPlayer
{
    [JsInterface("drawing")]
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
            public JsFunc Callback;
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
        public void register(string category, JsFunc fn)
        {
            // this may be called multiple times on the same engine
            //engine.OnDestroy -= Engine_OnDestroy;
            //engine.OnDestroy += Engine_OnDestroy;

            _cbs.Add(new CallbackRecord
            {
                Category = category.ToLower(),
                Callback = fn
            });
        }

        /// <summary>
        /// Removes a callback for drawing.
        /// </summary>
        /// <param name="fn">The callback to remove.</param>
        public void unregister(JsFunc fn)
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
        private void Engine_OnDestroy(IJsExecutionContext jsExecutionContext)
        {
            //engine.OnDestroy -= Engine_OnDestroy;

            // remove all callbacks related to this engine
            /*
            for (var i = _cbs.Count - 1; i >= 0; i--)
            {
                if (_cbs[i].Engine == engine)
                {
                    _cbs.RemoveAt(i);
                }
            }
            */
        }
    }
}
