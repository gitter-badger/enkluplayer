using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public interface IQrReaderService
    {
        event Action<string> OnRead;

        IAsyncToken<Void> Start();
        IAsyncToken<Void> Stop();
    }
}