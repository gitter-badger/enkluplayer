
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Updates the visual components related to a widget
    /// </summary>
    public class PrimitiveRenderer : MonoBehaviour
    {
        /// <summary>
        /// Cached list of materials
        /// </summary>
        private Material[] _cachedRendererMaterials;

        /// <summary>
        /// Source widget.
        /// </summary>
        public WidgetPrimitive Source;

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
        /// String override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("WidgetRenderer[{0}]", Source != null ? Source.Widget.GameObject.name : name);
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (Source == null)
            {
                Log.Warning(this, "Missing source 'Widget' for {0}!", this);
            }
        }

        /// <summary>
        /// Frame base dupdate.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (Source == null
             || Source.Widget == null)
            {
                return;
            }

            var color = Source.Color;
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