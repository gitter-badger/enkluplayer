using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Updates the visual components related to a <c>Widget</c>.
    /// </summary>
    public class WidgetRenderer : MonoBehaviour
    {
        /// <summary>
        /// Cached list of materials.
        /// </summary>
        private Material[] _cachedRendererMaterials;

        /// <summary>
        /// Source widget.
        /// </summary>
        private Widget _source;

        /// <summary>
        /// Local color
        /// </summary>
        private Col4 _localColor = Col4.White;
        
        /// <summary>
        /// Target graphic (Unity UI rendering system).
        /// </summary>
        public Graphic Graphic;

        /// <summary>
        /// Target canvas renderer (Lower level Unity UI rendering system).
        /// </summary>
        public CanvasRenderer CanvasRenderer;

        /// <summary>
        /// Target renderer (Unity general rendering system).
        /// </summary>
        public Renderer Renderer;

        /// <summary>
        /// Target renderer (Unity general rendering system).
        /// </summary>
        public Material Material;

        /// <summary>
        /// Name of the color in the primary material of the target renderer.
        /// </summary>
        public string MaterialColorName = "_Color";

        /// <summary>
        /// Local color accessor
        /// </summary>
        public Col4 LocalColor
        {
            get { return _localColor; }
            set { _localColor = value; }
        }

        /// <summary>
        /// String override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "WidgetRenderer[{0}]",
                _source != null
                    ? _source.GameObject.name
                    : name);
        }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="widget">Associated widget.</param>
        public void Initialize(Widget widget)
        {
            _source = widget;
        }

        /// <summary>
        /// Frame base dupdate.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (null == _source || !_source.IsLoaded)
            {
                return;
            }

            var color = _source.Color * _localColor;
            if (Graphic != null)
            {
                Graphic.color = color.ToColor();
            }

            if (CanvasRenderer != null)
            {
                CanvasRenderer.SetAlpha(color.a);
            }

            UpdateRenderer(color);

            if (Material != null)
            {
                Material.SetColor(MaterialColorName, color.ToColor());
            }
        }

        /// <summary>
        /// Updates the renderer.
        /// </summary>
        private void UpdateRenderer(Col4 color)
        {
            if (string.IsNullOrEmpty(MaterialColorName)
             || Renderer == null)
            {
                return;
            }

            if (_cachedRendererMaterials == null)
            {
                _cachedRendererMaterials = Renderer.materials;
            }

            if (_cachedRendererMaterials.Length > 0)
            {
                for (int j = 0, jCount = _cachedRendererMaterials.Length; j < jCount; ++j)
                {
                    var material = _cachedRendererMaterials[j];
                    if (material != null)
                    {
                        material.SetColor(
                            MaterialColorName,
                            color.ToColor());
                    }
                }
            }
        }
    }
}