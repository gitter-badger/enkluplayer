namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Describes an object that can handle an <c>IUXEvent</c>.
    /// </summary>
    public interface IIUXEventDelegate
    {
        /// <summary>
        /// Called when a matching event has been fired.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        bool OnEvent(IUXEvent @event);
    }
}