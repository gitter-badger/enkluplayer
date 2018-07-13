using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Draws an outline of a model.
    /// </summary>
    [InjectVine("Asset.Reload")]
    public class ModelLoadingOutline : InjectableIUXController
    {
        /// <summary>
        /// For drawing.
        /// </summary>
        private static Material _lineMaterial;

        /// <summary>
        /// Cached positions of the edge of the bounds.
        /// </summary>
        private Vector3[] _positions;

        /// <summary>
        /// True iff there was an error.
        /// </summary>
        private bool _isError;

        /// <summary>
        /// Retries download.
        /// </summary>
        public event Action OnRetry;

        /// <summary>
        /// The refresh button.
        /// </summary>
        public ButtonWidget BtnRefresh
        {
            get { return (ButtonWidget) Root; }
        }
        
        /// <summary>
        /// Called when there is a loading error.
        /// </summary>
        public void ShowError(string error)
        {
            _isError = true;

            BtnRefresh.LocalVisible = true;
        }

        /// <summary>
        /// Hides error.
        /// </summary>
        public void HideError()
        {
            _isError = false;

            BtnRefresh.LocalVisible = false;
        }

        /// <summary>
        /// Initializes with model bounds in world space.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        public void Init(Bounds bounds)
        {
            HideError();

            var trans = BtnRefresh.GameObject.transform;
            trans.localPosition = bounds.center;

            _isError = false;
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

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnRefresh.Activator.OnActivated += _ =>
            {
                if (null != OnRetry)
                {
                    OnRetry();
                }
            };
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            BtnRefresh.GameObject.transform.forward = Camera.main.transform.forward;
            BtnRefresh.Schema.Set("scale", new Vec3(
                5 / transform.lossyScale.x,
                5 / transform.lossyScale.y,
                5 / transform.lossyScale.z));
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
                    GL.Color(_isError ? Color.red : Color.white);

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