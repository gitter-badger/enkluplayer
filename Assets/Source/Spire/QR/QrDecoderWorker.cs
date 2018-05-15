#if UNITY_IOS || NETFX_CORE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Worker that decodes QR codes and sychronizes with main thread.
    /// </summary>
    public class QrDecoderWorker
    {
        /// <summary>
        /// Internal bookkeeping
        /// </summary>
        public class QrDecoderRecord
        {
            /// <summary>
            /// Generates unique ids.
            /// </summary>
            private static int IDS = 0;

            /// <summary>
            /// Unique id.
            /// </summary>
            public readonly int Id = IDS++;

            public string Path;
        }

        /// <summary>
        /// Internal result structure.
        /// </summary>
        private class QrDecoderResult
        {
            /// <summary>
            /// Id of frame.
            /// </summary>
            public int Id;

            /// <summary>
            /// True iff successful.
            /// </summary>
            public bool Success;

            /// <summary>
            /// The Qr value.
            /// </summary>
            public string Value;
        }

        /// <summary>
        /// Records to decode.
        /// </summary>
        private readonly Queue<QrDecoderRecord> _records = new Queue<QrDecoderRecord>();

        /// <summary>
        /// Results of decode.
        /// </summary>
        private readonly List<QrDecoderResult> _results = new List<QrDecoderResult>();

        /// <summary>
        /// Separate buffer for reading on main thread.
        /// </summary>
        private readonly List<QrDecoderResult> _resultsReadBuffer = new List<QrDecoderResult>();

        /// <summary>
        /// Object to lock records queue.
        /// </summary>
        private readonly object _queueLock = new object();

        /// <summary>
        /// True iff the worker is alive.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Reusable byte buffer.
        /// </summary>
        private byte[] _buffer = new byte[0];

        /// <summary>
        /// Called when a QR code is successfully read. Guaranteed to be called on main thread.
        /// </summary>
        public event Action<int, string> OnSuccess;

        /// <summary>
        /// Called when a QR code could not be read. Guaranteed to be called on main thread.
        /// </summary>
        public event Action<int> OnFail;

        /// <summary>
        /// Constructor.
        /// </summary>
        public QrDecoderWorker(IBootstrapper bootstrapper)
        {
            _isAlive = true;

            // start a poll
            bootstrapper.BootstrapCoroutine(Poll());
        }
        
        /// <summary>
        /// Enqueues a texture to read.
        /// </summary>
        public int Enqueue(string path)
        {
            var record = new QrDecoderRecord
            {
                Path = path
            };
            
            lock (_queueLock)
            {
                _records.Enqueue(record);

                Monitor.Pulse(_queueLock);
            }

            return record.Id;
        }
        
        /// <summary>
        /// Starts the worker.
        /// </summary>
        public void Start()
        {
            Log.Info(this, "Starting decode thread...");
            
            lock (_queueLock)
            {
                while (_isAlive)
                {
                    Monitor.Wait(_queueLock);

                    if (!_isAlive)
                    {
                        break;
                    }

                    var record = _records.Dequeue();
#if UNITY_IOS
                    /*
                    Marshal.Copy(record.Ptr, _buffer, 0, size);

                    var result = _reader.Decode(_buffer, record.Width, record.Height, RGBLuminanceSource.BitmapFormat.BGRA32);

                    lock (_results)
                    {
                        _results.Add(new QrDecoderResult
                        {
                            Id = record.Id,
                            Success = null != result,
                            Value = null != result ? result.Text : string.Empty
                        });
                    }*/
#else
                    lock (_results)
                    {
                        _results.Add(new QrDecoderResult
                        {
                            Id = record.Id,
                            Success = false
                        });
                    }
#endif
                }
            }

            Log.Info(this, "Shut down decode thread...");
        }

        /// <summary>
        /// Stops the worker.
        /// </summary>
        public void Stop()
        {
            _isAlive = false;

            lock (_queueLock)
            {
                Monitor.Pulse(_queueLock);
            }
        }

        /// <summary>
        /// Polls for results on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Poll()
        {
            while (_isAlive)
            {
                lock (_results)
                {
                    _resultsReadBuffer.AddRange(_results);
                    _results.Clear();
                }

                for (var i = 0; i < _resultsReadBuffer.Count; i++)
                {
                    var result = _resultsReadBuffer[i];
                    if (result.Success)
                    {
                        if (null != OnSuccess)
                        {
                            OnSuccess(result.Id, result.Value);
                        }
                    }
                    else if (null != OnFail)
                    {
                        OnFail(result.Id);
                    }
                }
                _resultsReadBuffer.Clear();

                yield return null;
            }
        }
    }
}

#endif