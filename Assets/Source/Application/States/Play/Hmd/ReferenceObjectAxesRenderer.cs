using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders axes for reference object.
    /// </summary>
    public class ReferenceObjectAxesRenderer : MonoBehaviour
    {
        /// <summary>
        /// For drawing.
        /// </summary>
        private static Material _lineMaterial;

        /// <inheritdoc cref="MonoBehaviour" />
        private void Awake()
        {
            CreateLineMaterial();
            _lineMaterial.SetPass(0);
        }

        /// <summary>
        /// Lazily creates material for drawing lines.
        /// </summary>
        private static void CreateLineMaterial()
        {
            if (!_lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                var shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;

                // Turn on alpha blending
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                // Turn backface culling off
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

                // Turn off depth writes
                _lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void OnRenderObject()
        {
            GL.PushMatrix();
            {
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
                GL.MultMatrix(transform.localToWorldMatrix);
                GL.Begin(GL.LINES);
                {
                    //x-axis
                    GL.Color(Color.red);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(1, 0, 0));

                    //y-axis
                    GL.Color(Color.green);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(0, 1, 0));

                    //z-axis
                    GL.Color(Color.blue);
                    GL.Vertex(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(0, 0, -1));
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}