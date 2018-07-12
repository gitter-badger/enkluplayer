using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class HierarchyLineRenderer : InjectableMonoBehaviour
    {
        private Material _material;

        [Inject]
        public IAppSceneManager Scenes { get; set; }

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
            _material.SetPass(0);

            GL.PushMatrix();
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(0.5f * Color.white);
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
            GL.Vertex(child.position);
            GL.Vertex(parent.position);
        }
    }
}