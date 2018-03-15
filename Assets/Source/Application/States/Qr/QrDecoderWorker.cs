#if NETFX_CORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using ZXing;

namespace CreateAR.SpirePlayer
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

            /// <summary>
            /// Color.
            /// </summary>
            public Color32[] Colors;

            /// <summary>
            /// Width of image.
            /// </summary>
            public int Width;

            /// <summary>
            /// Height of image.
            /// </summary>
            public int Height;
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
        /// Qr reader implementation.
        /// </summary>
        private readonly IBarcodeReader _reader = new BarcodeReader();

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
        /// <param name="colors">The colors.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns></returns>
        public int Enqueue(Color32[] colors, int width, int height)
        {
            var record = new QrDecoderRecord
            {
                Colors = colors,
                Width = width,
                Height = height
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
                    var result = _reader.Decode(record.Colors, record.Width, record.Height);

                    lock (_results)
                    {
                        _results.Add(new QrDecoderResult
                        {
                            Id = record.Id,
                            Success = null != result,
                            Value = null != result ? result.Text : string.Empty
                        });
                    }
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