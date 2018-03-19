using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ModelLoadingOutline : MonoBehaviour
    {
        /// <summary>
        /// For drawing.
        /// </summary>
        private static Material _lineMaterial;

        private Vector3[] _positions;

        public void Init(Bounds bounds)
        {
            _positions = new[]
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),

                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
            };
        }

        /// <summary>
        /// Render!
        /// </summary>
        private void OnRenderObject()
        {
            CreateLineMaterial();

            _lineMaterial.SetPass(0);
            
            GL.PushMatrix();
            {
                GL.MultMatrix(transform.localToWorldMatrix);
                GL.Begin(GL.LINES);
                {
                    var pos = _positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);

                    pos = _positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);
                    pos = _positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);
                }
                GL.End();
            }
            GL.PopMatrix();
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
        }
    }
}