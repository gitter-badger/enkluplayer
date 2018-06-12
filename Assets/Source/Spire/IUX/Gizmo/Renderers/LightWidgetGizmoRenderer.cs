﻿namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders a gizmo for a light.
    /// </summary>
    public class LightWidgetGizmoRenderer : MonoBehaviourGizmoRenderer
    {
        /// <summary>
        /// Renders a texture to screen.
        /// </summary>
        public GizmoTextureRenderer TextureRenderer;

        /// <inheritdoc />
        public override void Initialize(Element element)
        {
            base.Initialize(element);
            
            element.Schema.Set("visible", true);

            TextureRenderer.enabled = true;
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        private void Awake()
        {
            TextureRenderer.enabled = false;
        }
    }
}