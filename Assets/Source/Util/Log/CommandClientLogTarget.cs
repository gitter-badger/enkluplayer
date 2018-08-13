using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>ILogTarget</c> implementation that forwards logs to a list of <c>ICommandClient</c> implementations.
    /// </summary>
    public class CommandClientLogTarget : ILogTarget
    {
        /// <summary>
        /// Formats logs.
        /// </summary>
        private readonly ILogFormatter _formatter;

        /// <summary>
        /// The list of clients to forward logs to.
        /// </summary>
        private readonly List<ICommandClient> _clients = new List<ICommandClient>();

        /// <summary>
        /// Sub-filter.
        /// </summary>
        public LogLevel Filter { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="formatter">The formatter to use when forwarding logs.</param>
        public CommandClientLogTarget(ILogFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Adds a client to forward logs to. Qutomatically removes the client
        /// when upon disconnection.
        /// </summary>
        /// <param name="client">The client to track.</param>
        public void Add(ICommandClient client)
        {
            if (_clients.Contains(client))
            {
                return;
            }

            _clients.Add(client);
            client.OnClosed += Client_OnClosed;
        }

        /// <summary>
        /// Stops tracking a client.
        /// </summary>
        /// <param name="client"></param>
        public void Remove(ICommandClient client)
        {
            if (_clients.Remove(client))
            {
                client.OnClosed -= Client_OnClosed;
            }
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            if (level < Filter)
            {
                return;
            }

            for (var i = 0; i < _clients.Count; i++)
            {
                try
                {
                    _clients[i].Send(_formatter.Format(level, caller, message));
                }
                catch
                {
                    // TODO: count # of failed sends and remove this client if necessary
                }
            }
        }

        /// <summary>
        /// Called when the client connection is closed.
        /// </summary>
        /// <param name="client">The client.</param>
        private void Client_OnClosed(ICommandClient client)
        {
            _clients.Remove(client);
        }
    }
}