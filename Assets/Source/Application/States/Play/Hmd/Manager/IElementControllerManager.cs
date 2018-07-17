namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// This object tracks Elements using filters, and manages the resulting
    /// elements' components.
    /// </summary>
    public interface IElementControllerManager
    {
        /// <summary>
        /// True iff the controller manager is actively tracking elements.
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Retrieves or creates a group by tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        IElementControllerGroup Group(string tag);

        /// <summary>
        /// Deactivates all matching groups.
        /// </summary>
        /// <param name="tags">List of tags.</param>
        void Deactivate(params string[] tags);

        /// <summary>
        /// Activates all matching groups.
        /// </summary>
        /// <param name="tags">List of tags.</param>
        void Activate(params string[] tags);

        /// <summary>
        /// Destroys all groups.
        /// </summary>
        void Release();
    }
}