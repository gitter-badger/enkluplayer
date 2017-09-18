namespace CreateAR.Spire
{
    /// <summary>
    /// Bridge between Unity and hosting application.
    /// </summary>
    public interface IBridge
    {
        /// <summary>
        /// Tells the webpage that the application is ready.
        /// </summary>
        void BroadcastReady();

        /// <summary>
        /// Binds a message type to a Type.
        /// </summary>
        /// <typeparam name="T">The type with which to parse the event.</typeparam>
        /// <param name="messageTypeString">The message type.</param>
        /// <param name="messageTypeInt">The message type to push onto the <c>IMessageRouter</c>.</param>
        void Bind<T>(string messageTypeString, int messageTypeInt);

        /// <summary>
        /// Unbinds an event. See Bind.
        /// </summary>
        void Unbind<T>(string messageTypeString, int messageTypeInt);
    }
}