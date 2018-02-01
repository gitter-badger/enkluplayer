namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that creates <c>Content</c>.
    /// </summary>
    public interface IContentFactory
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="content">All content.</param>
        /// <param name="data">The <c>ContentData</c> to create the <c>Content</c> with.</param>
        /// <returns></returns>
        ContentWidget Instance(IContentManager content, ContentData data);
    }
}