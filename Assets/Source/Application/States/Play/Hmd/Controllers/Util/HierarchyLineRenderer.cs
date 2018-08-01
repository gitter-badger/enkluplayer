using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Renders lines for the hierarchy.
    /// </summary>
    public class HierarchyLineRenderer : InjectableMonoBehaviour
    {
        /// <summary>
        /// Material to draw with.
        /// </summary>
        private Material _material;

        /// <summary>
        /// Camera forward.
        /// </summary>
        private Vector3 _cameraForward;

        /// <summary>
        /// Manages scenes.
        /// </summary>
        [Inject]
        public IAppSceneManager Scenes { get; set; }

        /// <summary>
        /// Length of the arrow's sides.
        /// </summary>
        public float ArrowSize = 0.1f;

        /// <summary>
        /// How far away from base to make arrow.
        /// </summary>
        public float ArrowDelta = 0.3f;

        /// <summary>
        /// Color when selected.
        /// </summary>
        public Color SelectedColor = Color.yellow;

        /// <summary>
        /// Default color.
        /// </summary>
        public Color Color = 0.5f * Color.white;

        /// <summary>
        /// The selected element.
        /// </summary>
        public Element Selected { get; set; }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            _material = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _material.SetInt("_ZWrite", 0);
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void OnPostRender()
        {
            _cameraForward = Camera.main.transform.forward;
            _material.SetPass(0);

            GL.PushMatrix();
            try
            {
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
                GL.Begin(GL.LINES);
                try
                {
                    // draw each scene
                    var all = Scenes.All;
                    for (var i = 0; i < all.Length; i++)
                    {
                        var scene = all[i];
                        var root = Scenes.Root(scene);

                        Draw(root, null);
                    }
                } catch { }
                GL.End();
            } catch { }
            GL.PopMatrix();
        }

        /// <summary>
        /// Recursive drawing method, starting from the root of the scene.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="lastUnityParent">The last element that was also a unity element.</param>
        private void Draw(Element root, IUnityElement lastUnityParent)
        {
            var unityRoot = root as IUnityElement;
            if (null != unityRoot)
            {
                lastUnityParent = unityRoot;
            }

            var children = root.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                var child = children[i];
                var unityChild = child as IUnityElement;
                if (null != unityChild
                    && null != lastUnityParent
                    && unityChild.GameObject
                    && lastUnityParent.GameObject)
                {
                    Draw(
                        unityChild.GameObject.transform,
                        lastUnityParent.GameObject.transform,
                        child == Selected ? SelectedColor : Color);
                }

                Draw(child, lastUnityParent);
            }
        }

        /// <summary>
        /// Draws an arrow from child to parent.
        /// </summary>
        /// <param name="child">Child transform.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="color">The color to draw.</param>
        private void Draw(Transform child, Transform parent, Color color)
        {
            var a = child.position;
            var b = parent.position;
            GL.Color(color);
            GL.Vertex(a);
            GL.Vertex(b);

            var d = (b - a).normalized;
            var right = Vector3.Cross(_cameraForward, d);
            
            GL.Vertex(a + ArrowDelta * d);
            GL.Vertex(a + ArrowDelta * d + ArrowSize * (right - d).normalized);
            GL.Vertex(a + ArrowDelta * d);
            GL.Vertex(a + ArrowDelta * d - ArrowSize * (right + d).normalized);
        }
    }
}