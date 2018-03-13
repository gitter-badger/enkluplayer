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
    public class QrDecoderWorker
    {
        public class QrDecoderRecord
        {
            private static int IDS = 0;

            public readonly int Id = IDS++;
            public Color32[] Colors;
            public int Width;
            public int Height;
        }

        private class QrDecoderResult
        {
            public int Id;
            public bool Success;
            public string Value;
        }

        private readonly IBarcodeReader _reader = new BarcodeReader();
        private readonly Queue<QrDecoderRecord> _records = new Queue<QrDecoderRecord>();
        private readonly List<QrDecoderResult> _results = new List<QrDecoderResult>();
        private readonly List<QrDecoderResult> _resultsReadBuffer = new List<QrDecoderResult>();
        private readonly IBootstrapper _bootstrapper;
        
        private readonly object _queueLock = new object();

        private bool _isAlive;

        public event Action<int, string> OnSuccess;
        public event Action<int> OnFail;

        public QrDecoderWorker(IBootstrapper bootstrapper)
        {
            _isAlive = true;

            _bootstrapper = bootstrapper;
            _bootstrapper.BootstrapCoroutine(Poll());
        }
        
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

        public void Stop()
        {
            _isAlive = false;
        }

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