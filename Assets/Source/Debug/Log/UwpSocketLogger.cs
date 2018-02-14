#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Logs to a websocket.
    /// </summary>
    public class UwpSocketLogger : ILogTarget, IDisposable
    {
        /// <summary>
        /// Websocket.
        /// </summary>
        private MessageWebSocket _socket;

        /// <summary>
        /// Writes data.
        /// </summary>
        private DataWriter _writer;

        /// <summary>
        /// Producer/Consumer queue.
        /// </summary>
        private readonly Queue<string> _queue = new Queue<string>();

        /// <summary>
        /// Allows for cancellation.
        /// </summary>
        private CancellationTokenSource _cancelSource;

        /// <summary>
        /// Underlyting task.
        /// </summary>
        private Task _task;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UwpSocketLogger()
        {
            //
        }

        /// <summary>
        /// Creates a new UwpSocketLogger and tries to connect immediately.
        /// </summary>
        /// <param name="identity">How to identify.</param>
        /// <param name="uri">Uri to connect to.</param>
        public UwpSocketLogger(string identity, Uri uri)
        {
            Connect(identity, uri);
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Disconnects from the endpoint.
        /// </summary>
        public void Disconnect()
        {
            if (null != _task)
            {
                _cancelSource.Cancel();
                _task.Wait();
                _task = null;
            }

            if (null != _writer)
            {
                _writer.DetachStream();
                _writer.Dispose();
                _writer = null;
            }

            if (null != _socket)
            {
                _socket.Close(0, "Disconnecting.");
                _socket.Dispose();
                _socket = null;
            }
        }

        /// <summary>
        /// Connects to the endpoint.
        /// </summary>
        /// <param name="identity">How to identify with the log server.</param>
        /// <param name="uri">The URI to connect to.</param>
        public async void Connect(string identity, Uri uri)
        {
            Disconnect();

            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.Closed += Socket_OnClosed;

            try
            {
                await _socket.ConnectAsync(uri);

                _cancelSource = new CancellationTokenSource();
                _writer = new DataWriter(_socket.OutputStream)
                {
                    UnicodeEncoding = UnicodeEncoding.Utf8
                };

                // start a long-running consumer task
                _task = Task.Factory.StartNew(
                    Send,
                    _cancelSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                Buffer("Identify", identity);
                Buffer("Info", "Connected.");

            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not connect to {0} : {1}.",
                    uri,
                    exception);
            }
        }
        
        /// <inheritdoc cref="ILogTarget"/>
        public void OnLog(LogLevel level, object caller, string message)
        {
            string command;
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Info:
                case LogLevel.Debug:
                {
                    command = level.ToString();
                    break;
                }
                case LogLevel.Fatal:
                {
                    command = LogLevel.Error.ToString();
                    break;
                }
                case LogLevel.Warning:
                {
                    command = "Warn";
                    break;
                }
                default:
                {
                    command = "Debug";
                    break;
                }
            }

            Buffer(command, message);
        }

        /// <summary>
        /// Called when the socket has been closed.
        /// </summary>
        private void Socket_OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            Log.Warning(this, "Disconnected from log client. Trying reconnect.");
        }

        /// <summary>
        /// Buffers a command for sending.
        /// </summary>
        /// <param name="command">NaturalLog command.</param>
        /// <param name="value">Value of the command.</param>
        private void Buffer(string command, string value)
        {
            var message = "{" + command + "}:" + value;

            _queue.Enqueue(message);
        }

        /// <summary>
        /// Longrunning consumer.
        /// </summary>
        /// <returns></returns>
        private async Task Send()
        {
            if (null == _writer)
            {
                return;
            }

            while (!_cancelSource.IsCancellationRequested)
            {
                var message = _queue.Dequeue();

                _writer.WriteString(message);

                await _writer.StoreAsync();
                await _writer.FlushAsync();
            }
        }
    }
}
#endif