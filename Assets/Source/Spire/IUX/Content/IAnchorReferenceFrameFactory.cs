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
        /// <param name="anchor">The Anchor to create it for.</param>
        /// <returns></returns>
        IAnchorReferenceFrame Instance(Anchor anchor);
    }
}