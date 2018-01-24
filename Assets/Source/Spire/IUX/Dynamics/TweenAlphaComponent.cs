using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Component that tweens alpha.
    /// </summary>
    public class TweenAlphaComponent : MonoBehaviour, IWidgetComponent
    {
        /// <summary>
        /// Graphic.
        /// </summary>
        public Graphic Graphic;

        /// <summary>
        /// CanvasRenderer.
        /// </summary>
        public CanvasRenderer CanvasRenderer;

        /// <summary>
        /// Particle System.
        /// </summary>
        public ParticleSystem ParticleSystem;

        /// <summary>
        /// Renderer.
        /// </summary>
        public Renderer Renderer;

        /// <summary>
        /// Material.
        /// </summary>
        public Material Material;

        /// <summary>
        /// Name for renderer material.
        /// </summary>
        public string MaterialColorName = "_Color";

        /// <summary>
        /// If true, excludes color from tween.
        /// </summary>
        public bool ExcludeColor;

        /// <summary>
        /// For modulating against widget color.
        /// </summary>
        public Color GraphicStartColor { get; private set; }

        /// <summary>
        /// The Widget to affect.
        /// </summary>
        public Widget Widget { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        private void Awake()
        {
            if (Graphic != null)
            {
                GraphicStartColor = Graphic.color;
            }
            else
            {
                GraphicStartColor = Color.white;
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void LateUpdate()
        {
            if (Widget != null)
            {
                var widgetColor = Widget.Color;
                if (ExcludeColor)
                {
                    widgetColor.r = GraphicStartColor.r;
                    widgetColor.g = GraphicStartColor.g;
                    widgetColor.b = GraphicStartColor.b;
                }

                if (Graphic != null)
                {
                    var color = widgetColor.ToColor() * GraphicStartColor;
                    Graphic.color = color;
                }

                if (CanvasRenderer != null)
                {
                    CanvasRenderer.SetAlpha(widgetColor.a);
                }

                if (ParticleSystem != null)
                {
                    var color = widgetColor;
                    var mainModule = ParticleSystem.main;
                    
                    mainModule.startColor = color.ToColor();
                }

                if (Renderer != null && Renderer.material != null)
                {
                    Renderer.material.SetColor(
                        MaterialColorName,
                        widgetColor.ToColor());
                }

                if (Material != null)
                {
                    Material.SetColor(
                        MaterialColorName,
                        widgetColor.ToColor());
                }
            }
        }
    }
}