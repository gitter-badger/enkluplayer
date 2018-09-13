namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for receiving messages from a websocket.
    /// </summary>
    public interface IUwpWebsocketService
    {
        /// <summary>
        /// Called when websocket has been opened.
        /// </summary>
        void OnOpen();

        /// <summary>
        /// Called when a message has been received.
        /// </summary>
        /// <param name="message">Message that was received.</param>
        void OnMessage(string message);

        /// <summary>
        /// Called when the websocket has been closed.
        /// </summary>
        void OnClose();
    }
}
