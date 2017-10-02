namespace CreateAR.Spire
{
    /// <summary>
    /// Describes an object that creates <c>IAnchorReferenceFrame</c> implementations.
    /// </summary>
    public interface IAnchorReferenceFrameFactory
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="content">Manages content.</param>
        /// <param name="anchor">The Anchor to create it for.</param>
        /// <param name="type">The type of anchor to create.</param>
        /// <returns></returns>
        IAnchorReferenceFrame Instance(
            IContentManager content,
            Anchor anchor,
            AnchorType type);
    }
}