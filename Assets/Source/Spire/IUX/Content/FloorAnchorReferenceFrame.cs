namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Frame of reference for the Floor.
    /// </summary>
    public class FloorAnchorReferenceFrame : IAnchorReferenceFrame
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IIntentionManager _intention;
        
        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Forward { get { return _intention.Forward; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Up { get { return _intention.Up; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Right { get { return _intention.Right; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloorAnchorReferenceFrame(IIntentionManager intention)
        {
            _intention = intention;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Attach(Anchor anchor)
        {
            var origin = _intention.Origin;

            var transform = anchor.transform;
            transform.SetParent(null);
            transform.position = origin.ToVector();
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Update(float dt)
        {
            //
        }
    }
}