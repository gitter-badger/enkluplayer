using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class SubMenuWidgetLineRenderer : MonoBehaviour
    {
        private readonly List<Transform> _transforms = new List<Transform>();

        private Material _material;

        private ElementSchemaProp<bool> _visibleProp;

        public void Initialize(MenuWidget menu)
        {
            _transforms.Clear();

            var children = menu.LayoutChildren;
            for (var i = 0; i < children.Count; i++)
            {
                var unityChild = children[i] as IUnityElement;
                if (null != unityChild)
                {
                    _transforms.Add(unityChild.GameObject.transform);
                }
            }

            _visibleProp = menu.Schema.Get<bool>("visible");
        }

        public void Uninitialize()
        {
            _transforms.Clear();
        }

        private void Awake()
        {
            _material = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _material.SetInt("_ZWrite", 0);
        }

        private void OnRenderObject()
        {
            if (null == _visibleProp || !_visibleProp.Value)
            {
                return;
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

                        GL.Vertex(transform.position);
                        GL.Vertex(trans.position);
                    }
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}