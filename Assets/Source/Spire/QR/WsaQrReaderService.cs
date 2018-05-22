#if NETFX_CORE
using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Service that reads in QR codes from webcam. This implementation is for Wsa.
    /// </summary>
    public class WsaQrReaderService : IQrReaderService
    {
        private readonly IBootstrapper _bootstrapper;
        private readonly List<Action> _queuedActions = new List<Action>();
        private readonly List<Action> _queuedActionsReadBuffer = new List<Action>();

        private bool _isAlive;
        private bool _isReady;
        private AsyncToken<Void> _start;
        private AsyncToken<Void> _stop;
        
        public event Action<string> OnRead;

        public WsaQrReaderService(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            _start = new AsyncToken<Void>();
            
#if !UNITY_EDITOR

#endif

            return _start;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Stop()
        {
            _stop = new AsyncToken<Void>();

            _isAlive = false;
            
            return _stop;
        }
    }
}
#endif