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
    /// <summary>
    /// Uwp specific implementation for sending metrics to HostedGraphite.
    /// </summary>
    public class UwpHostedGraphiteMetricsTarget : IHostedGraphiteMetricsTarget, IDisposable
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
            private double _timestamp = 0.0;

            /// <summary>
            /// Tracks number of tries.
            /// </summary>
            private int _tries;

            /// <summary>
            /// True iff retried enough times.
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

                if (_timestamp < double.Epsilon)
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

        /// <summary>
        /// Queue of records that need to be sent off.
        /// </summary>
        private readonly BlockingCollection<Record> _queue = new BlockingCollection<Record>();

        /// <summary>
        /// Hostname so we can reconnect.
        /// </summary>
        private string _hostname;

        /// <summary>
        /// API key for sending to HostedGraphite.
        /// </summary>
        private string _key;

        /// <summary>
        /// The C# UDP socket API.
        /// </summary>
        private DatagramSocket _socket;

        /// <summary>
        /// For writing to a socket.
        /// </summary>
        private DataWriter _writer;
        
        /// <summary>
        /// Allows for cancelling the consumer task.
        /// </summary>
        private CancellationTokenSource _source;

        /// <inheritdoc />
        public void Setup(string hostname, string key)
        {
            _hostname = hostname;
            _key = key;

            Connect();
        }

        /// <inheritdoc />
        public void Send(string key, float value)
        {
            if (!_queue.TryAdd(new Record(key, value), 5))
            {
                Log.Warning(this, "Timed out queuing metric.");
            }
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
        ~UwpHostedGraphiteMetricsTarget()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// <c>IDisposable</c> pattern.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            _writer?.Dispose();
            _writer = null;

            _socket?.Dispose();
            _socket = null;

            _source?.Cancel();
            _source?.Dispose();
            _source = null;
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

                Log.Info(this, "Successfully connected to graphite.");

                _writer = new DataWriter(_socket.OutputStream);

                StartConsumer();
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not connect to HostedGraphite : {0}.", exception);
                
                // TODO: schedule a retry for later
            }
        }

        /// <summary>
        /// Starts a consumer task.
        /// </summary>
        private void StartConsumer()
        {
            // cancel the last consumer thread
            _source?.Cancel();
            _source?.Dispose();
            _source = null;

            // new cancellation source
            _source = new CancellationTokenSource();

            // create consumer
            try
            {
                var token = _source.Token;

                Log.Info(this, "Starting graphite consumer thread.");

                Task.Run(async () =>
                {
                    Log.Info(this, "Consumer thread started.");

                    var isReconnect = false;

                    // check if we've been cancelled
                    while (!token.IsCancellationRequested)
                    {
                        // take will block until there is something in the queue or the token has been cancelled
                        Record payload;
                        try
                        {
                            payload = _queue.Take(_source.Token);
                        }
                        catch
                        {
                            break;
                        }
                        
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
                                Log.Warning(this, "Socket died, reconnecting.");

                                // we need to reconnect
                                isReconnect = true;

                                // kill!
                                _writer?.Dispose();
                                _writer = null;

                                _socket?.Dispose();
                                _socket = null;

                                break;
                            }
                        }
                    }

                    if (isReconnect)
                    {
                        Connect();
                    }

                    // we don't really need to use token.ThrowIfCancellationRequested()
                }, token);
            }
            catch
            {
                // this will be called when the task has been cancelled
            }
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
            else
            {
                // TODO: schedule for retry later instead of immediately
                if (!_queue.TryAdd(payload, 5))
                {
                    Log.Warning(this, "Rescheduling metric timed out.");
                }
            }
        }
    }
}

#endif