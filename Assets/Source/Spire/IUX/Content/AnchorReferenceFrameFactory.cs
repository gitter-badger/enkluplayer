using System;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates <c>IAnchorReferenceFrame</c> implementations.
    /// </summary>
    public class AnchorReferenceFrameFactory : IAnchorReferenceFrameFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IIntentionManager _intention;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorReferenceFrameFactory(IIntentionManager intention)
        {
            _intention = intention;
        }

        /// <inheritdoc cref="IAnchorReferenceFrameFactory"/>
        public IAnchorReferenceFrame Instance(
            IContentManager content,
            AnchorType type)
        {
            switch (type)
            {
                case AnchorType.Floor:
                {
                    return new FloorAnchorReferenceFrame(_intention);
                }
                case AnchorType.Locator:
                {
                    return new LocatorAnchorReferenceFrame(
                        _intention,
                        content);
                }
                case AnchorType.None:
                {
                    return new NoneAnchorReferenceFrame();
                }
            }

            throw new NotImplementedException();
        }
    }
}