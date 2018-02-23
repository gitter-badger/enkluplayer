using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Connection that does nothing.
    /// </summary>
    public class PassthroughConnection : IConnection
    {
        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}