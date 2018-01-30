using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can handle an <c>IUXEvent</c>.
    /// </summary>
    public interface IIUXEventHandler
    {
        /// <summary>
        /// Called when a matching event has been fired.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        bool OnEvent(IUXEvent @event);
    }
}