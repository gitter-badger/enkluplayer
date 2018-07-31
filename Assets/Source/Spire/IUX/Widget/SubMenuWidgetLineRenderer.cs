using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders lines between menu buttons.
    /// </summary>
    public class SubMenuWidgetLineRenderer : MonoBehaviour
    {
        /// <summary>
        /// Cached transforms to render lines to.
        /// </summary>
        private readonly List<Transform> _transforms = new List<Transform>();

        /// <summary>
        /// Material to render with.
        /// </summary>
        private Material _material;

        /// <summary>
        /// The visibility property.
        /// </summary>
        private ElementSchemaProp<bool> _visibleProp;

        /// <summary>
        /// True iff initialized.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// So that the lines do not intersect the buttons.
        /// </summary>
        public float Offset = 0.05f;

        /// <summary>
        /// Menu.
        /// </summary>
        private MenuWidget _menu;

        /// <summary>
        /// Starts the renderer with a menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        public void Initialize(MenuWidget menu)
        {
            _menu = menu;
            _visibleProp = menu.Schema.Get<bool>("visible");
            _isInitialized = true;
        }

        /// <summary>
        /// Shuts off rendering.
        /// </summary>
        public void Uninitialize()
        {
            _transforms.Clear();
            _isInitialized = false;
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
        private void OnRenderObject()
        {
            if (!_isInitialized || !_visibleProp.Value)
            {
                return;
            }

            // gather transforms
            _transforms.Clear();
            var children = _menu.LayoutChildren;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var unityChild = child as IUnityElement;
                if (null != unityChild && child.Schema.Get<bool>("visible").Value)
                {
                    _transforms.Add(unityChild.GameObject.transform);
                }
            }

            _material.SetPass(0);

            GL.PushMatrix();
            {
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
                GL.Begin(GL.LINES);
                GL.Color(0.4f * Color.white);
                {
                    for (var i = 0; i < _transforms.Count; i++)
                    {
                        var trans = _transforms[i];
                        var delta = trans.position - transform.position;
                        var d = delta.normalized;

                        GL.Vertex(transform.position + Offset * d);
                        GL.Vertex(trans.position - Offset * d);
                    }
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}