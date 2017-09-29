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
        private readonly IContentManager _content;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorReferenceFrameFactory(
            IntentionManager intention,
            IContentManager content)
        {
            _intention = intention;
            _content = content;
        }

        /// <inheritdoc cref="IAnchorReferenceFrameFactory"/>
        public IAnchorReferenceFrame Instance(Anchor anchor)
        {
            switch (anchor.Data.Type)
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
                        _content,
                        anchor);
                }
            }

            throw new NotImplementedException();
        }
    }
}