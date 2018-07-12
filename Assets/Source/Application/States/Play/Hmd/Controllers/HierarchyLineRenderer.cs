using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class HierarchyLineRenderer : InjectableMonoBehaviour
    {
        private Material _material;
        private Vector3 _cameraForward;

        [Inject]
        public IAppSceneManager Scenes { get; set; }

        public float ArrowSize = 0.1f;
        public float ArrowDelta = 0.3f;

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

        private void OnPostRender()
        {
            _cameraForward = Camera.main.transform.forward;
            _material.SetPass(0);

            GL.PushMatrix();
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(0.5f * new Color(1, 1, 1, 1));
            GL.Begin(GL.LINES);

            // draw each scene
            var all = Scenes.All;
            for (var i = 0; i < all.Length; i++)
            {
                var scene = all[i];
                var root = Scenes.Root(scene);

                Draw(root, null);
            }

            GL.End();
            GL.PopMatrix();
        }

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
                if (null != unityChild && null != lastUnityParent)
                {
                    Draw(
                        unityChild.GameObject.transform,
                        lastUnityParent.GameObject.transform);
                }

                Draw(child, lastUnityParent);
            }
        }

        private void Draw(Transform child, Transform parent)
        {
            var a = child.position;
            var b = parent.position;
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