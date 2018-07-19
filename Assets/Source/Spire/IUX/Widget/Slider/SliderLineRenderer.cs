using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class SliderLineRenderer : MonoBehaviour
    {
        private Material _lineMaterial;

        public Vector3 O;
        public Vector3 d;

        public Color Color = 0.25f * Color.white;

        private void Awake()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
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

        /// <inheritdoc cref="MonoBehaviour" />
        private void OnRenderObject()
        {
            GL.PushMatrix();
            {
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
                _lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                {
                    GL.Color(Color);

                    GL.Vertex(O + 1000f * d);
                    GL.Vertex(O - 1000f * d);
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}