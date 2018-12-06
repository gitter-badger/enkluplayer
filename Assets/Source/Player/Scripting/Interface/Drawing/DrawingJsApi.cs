using System.Collections.Generic;
using Jint;
using Jint.Native;
using UnityEngine;

using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer
{
    [JsInterface("drawing")]
    public class DrawingJsApi : MonoBehaviour
    {
        private class CallbackRecord
        {
            public Engine Engine;
            public string Category;
            public JsFunc Callback;
        }

        private readonly List<CallbackRecord> _cbs = new List<CallbackRecord>();

        private readonly ContextJsApi _context = new ContextJsApi();

        private string _filter = string.Empty;

        /// <summary>
        /// The material to draw with, programmatically generated.
        /// </summary>
        private Material _material;

        public void filter(string filter)
        {
            _filter = filter.ToLower();
        }
        
        public void render(Engine engine, string category, JsFunc fn)
        {
            _cbs.Add(new CallbackRecord
            {
                Engine = engine,
                Category = category.ToLower(),
                Callback = fn
            });
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
                        record.Callback(
                            JsValue.FromObject(record.Engine, this),
                            new[] {JsValue.FromObject(record.Engine, _context)});
                    }
                    catch
                    {
                        // 
                    }
                }

                _cbs.Clear();
            }
        }
    }
}
