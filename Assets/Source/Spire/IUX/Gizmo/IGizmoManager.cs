namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders gizmos.
    /// </summary>
    public interface IGizmoManager
    {
        /// <summary>
        /// Adds an element for rendering. Element is automatically untracked
        /// when destroyed.
        /// </summary>
        /// <param name="element">The element.</param>
        void Track(Element element);
    }
}