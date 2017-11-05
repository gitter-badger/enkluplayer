
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Updates the visual components related to a widget
    /// </summary>
    public class WidgetRenderer : MonoBehaviour
    {
        /// <summary>
        /// Cached list of materials
        /// </summary>
        private Material[] _cachedRendererMaterials;

        /// <summary>
        /// Source widget.
        /// </summary>
        public Widget Widget;

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
            return string.Format("WidgetRenderer[{0}]", Widget != null ? Widget.name : name);
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (Widget == null)
            {
                Log.Warning(this, "Missing source 'Widget' for {0}!", this);
            }
        }

        /// <summary>
        /// Frame base dupdate.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (Widget == null)
            {
                return;
            }

            if (Graphic != null)
            {
                Graphic.color = Widget.Color;
            }

            if (CanvasRenderer != null)
            {
                CanvasRenderer.SetAlpha(Widget.Color.a);
            }

            UpdateRenderer();

            if (Material != null)
            {
                Material.SetColor(MaterialColorName, Widget.Color);
            }
        }

        /// <summary>
        /// Updates the renderer.
        /// </summary>
        private void UpdateRenderer()
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
                var color = Widget.Color;
                for (int j = 0, jCount = _cachedRendererMaterials.Length; j < jCount; ++j)
                {
                    var material = _cachedRendererMaterials[j];
                    if (material != null)
                    {
                        material.SetColor(
                            MaterialColorName,
                            color);
                    }
                }
            }
        }
    }
}