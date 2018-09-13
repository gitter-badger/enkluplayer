using RLD;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Renders axes for reference object.
    /// </summary>
    public class ReferenceObjectAxesRenderer : MonoBehaviour, IRTObjectSelectionListener
    {
        /// <summary>
        /// For drawing.
        /// </summary>
        private Material _material;

        /// <inheritdoc />
        public bool OnCanBeSelected(ObjectSelectEventArgs selectArgs)
        {
            return false;
        }

        /// <inheritdoc />
        public void OnSelected(ObjectSelectEventArgs selectArgs)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void OnDeselected(ObjectDeselectEventArgs deselectArgs)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Awake()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            _material = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _material.SetInt("_ZWrite", 0);
        }
        
        /// <inheritdoc cref="MonoBehaviour" />
        private void OnRenderObject()
        {
            GL.PushMatrix();
            {
                GL.MultMatrix(transform.localToWorldMatrix);

                _material.SetPass(0);
                GL.Begin(GL.LINES);
                {
                    // x-axis
                    GL.Color(Color.red);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(1, 0, 0));

                    // y-axis
                    GL.Color(Color.green);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(0, 1, 0));

                    // z-axis
                    GL.Color(Color.blue);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(0, 0, 1));
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}