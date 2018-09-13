namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Renders gizmo for WorldAnchorWidget.
    /// </summary>
    public class WorldAnchorGizmoRenderer : MonoBehaviourGizmoRenderer
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