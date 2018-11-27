#if NETFX_CORE

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    public class DatagramController : IDisposable
    {
        /// <summary>
        /// Internal record.
        /// </summary>
        private class Record
        {
            /// <summary>
            /// Maximum number of retries.
            /// </summary>
            private const int MAX_RETRIES = 3;

            /// <summary>
            /// Key.
            /// </summary>
            private readonly string _key;

            /// <summary>
            /// Metric value.
            /// </summary>
            private readonly float _value;

            /// <summary>
            /// Timestamp is only set if retried.
            /// </summary>
            private double _timestamp;

            /// <summary>
            /// Tracks number of tries.
            /// </summary>
            private int _tries;

            /// <summary>
            /// Trie iff retried enough times.
            /// </summary>
            public bool IsExpired => _tries >= MAX_RETRIES;

            /// <summary>
            /// Constructor.
            /// </summary>
            public Record(string key, float value)
            {
                _key = key;
                _value = value;
            }

            /// <summary>
            /// Converts to bytes.
            /// </summary>
            /// <param name="appKey">The key to use.</param>
            /// <returns></returns>
            public byte[] ToBytes(string appKey)
            {
                string message;

                if (_timestamp > double.Epsilon)
                {
                    message = $"{appKey}.{_key} {_value:0.####}\n";
                }
                else
                {
                    message = $"{appKey}.{_key} {_value:0.####} {_timestamp:0.##}\n";
                }
                
                return Encoding.ASCII.GetBytes(message);
            }

            /// <summary>
            /// Makes this record to be retried.
            /// </summary>
            public void MarkForRetry()
            {
                _tries += 1;

                if (_timestamp < double.Epsilon)
                {
                    _timestamp = (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
                }
            }
        }

        private readonly string _hostname;
        private readonly string _key;
        private readonly ConcurrentQueue<Record> _queue = new ConcurrentQueue<Record>();

        private DatagramSocket _socket;

        private DataWriter _writer;

        private string _consumerId;
        private CancellationTokenSource _source;

        public DatagramController(string hostname, string key)
        {
            _hostname = hostname;
            _key = key;

            Connect();
        }

        public void Send(string key, float value)
        {
            _queue.Enqueue(new Record(key, value));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <c>IDisposable</c> pattern.
        /// </summary>
        ~DatagramController()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// <c>IDisposable</c> pattern.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            _writer?.Dispose();
            _socket?.Dispose();
            _source?.Cancel();
            _consumerId = string.Empty;
        }

        /// <summary>
        /// Points the UWP socket.
        /// </summary>
        private async void Connect()
        {
            try
            {
                Log.Info(this, "Connecting to host.");

                _socket = new DatagramSocket();
                await _socket.ConnectAsync(new HostName(_hostname), "2003");

                _writer = new DataWriter(_socket.OutputStream);

                Start();
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not connect to HostedGraphite : {0}.", exception);
                
                // TODO: schedule a retry
            }
        }

        /// <summary>
        /// Start consuming.
        /// </summary>
        private void Start()
        {
            var local = _consumerId = Guid.NewGuid().ToString();

            // cancel the last one
            _source?.Cancel();

            // new cancellation
            _source = new CancellationTokenSource();
            var token = _source.Token;

            // create consumer
            Task.Run(async () =>
            {
                var isReconnect = false;
                
                while (local == _consumerId && _queue.TryDequeue(out var payload))
                {
                    _writer.WriteBytes(payload.ToBytes(_key));

                    try
                    {
                        await _writer.StoreAsync();
                    }
                    catch (Exception exception)
                    {
                        // try again later
                        Reschedule(payload);

                        if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                        {
                            // we need to reconnect
                            isReconnect = true;

                            // kill!
                            _writer?.Dispose();
                            _socket?.Dispose();

                            break;
                        }
                    }
                }
                
                if (isReconnect)
                {
                    Connect();
                }
            }, token);
        }

        /// <summary>
        /// Reschedules this to be tried again later.
        /// </summary>
        /// <param name="payload">The payload to try later.</param>
        private void Reschedule(Record payload)
        {
            // MARK IT!
            payload.MarkForRetry();
            
            if (payload.IsExpired)
            {
                // discard
                Log.Warning(this, $"Could not send payload after retries. Discarding.");

                // TODO: write to disk?
            }

            // TODO: schedule for retry later
        }
    }
}

#endif