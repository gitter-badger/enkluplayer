#if UNITY_IOS

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Worker that decodes QR codes and sychronizes with main thread.
    /// </summary>
    public class IosQrDecoderWorker
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
            /// Path to the PNG.
            /// </summary>
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
        public IosQrDecoderWorker(IBootstrapper bootstrapper)
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
                    var decoded = IosQrNativeInterface.DecodeAtPath(record.Path);
                    
                    File.Delete(record.Path);

                    lock (_results)
                    {
                        _results.Add(new QrDecoderResult
                        {
                            Id = record.Id,
                            Success = !string.IsNullOrEmpty(decoded),
                            Value = decoded
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