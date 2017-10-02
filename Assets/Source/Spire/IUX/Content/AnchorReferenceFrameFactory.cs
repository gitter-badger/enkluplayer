using System;

namespace CreateAR.Spire
{
    /// <summary>
    /// Creates <c>IAnchorReferenceFrame</c> implementations.
    /// </summary>
    public class AnchorReferenceFrameFactory : IAnchorReferenceFrameFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IntentionManager _intention;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorReferenceFrameFactory(IntentionManager intention)
        {
            _intention = intention;
        }

        /// <inheritdoc cref="IAnchorReferenceFrameFactory"/>
        public IAnchorReferenceFrame Instance(
            IContentManager content,
            Anchor anchor,
            AnchorType type)
        {
            switch (type)
            {
                case AnchorType.Floor:
                {
                    return new FloorAnchorReferenceFrame(
                        _intention,
                        anchor);
                }
                case AnchorType.Locator:
                {
                    return new LocatorReferenceFrame(
                        _intention,
                        content,
                        anchor);
                }
            }

            throw new NotImplementedException();
        }
    }
}