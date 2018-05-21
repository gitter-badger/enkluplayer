namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Describes an object that renders gizmos for an element.
    /// </summary>
    public interface IGizmoRenderer
    {
        /// <summary>
        /// The element gizmos are being rendered for.
        /// </summary>
        Element Element { get; }

        /// <summary>
        /// Starts the renderer.
        /// </summary>
        /// <param name="element">The element to render gizmos for.</param>
        void Initialize(Element element);
        
        /// <summary>
        /// Stops the renderer.
        /// </summary>
        void Uninitialize();
    }
}
