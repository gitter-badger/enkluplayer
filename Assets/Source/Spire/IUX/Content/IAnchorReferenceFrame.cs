namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a frame of reference for an <c>Anchor</c>.
    /// </summary>
    public interface IAnchorReferenceFrame
    {
        /// <summary>
        /// Forward.
        /// </summary>
        Vec3 Forward { get; }

        /// <summary>
        /// Up.
        /// </summary>
        Vec3 Up { get; }

        /// <summary>
        /// Right.
        /// </summary>
        Vec3 Right { get; }

        /// <summary>
        /// Attaches to frame.
        /// </summary>
        void Attach(Anchor anchor);

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">Time, in seconds, that have passed since last frame.</param>
        void Update(float dt);
    }
}