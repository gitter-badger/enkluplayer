﻿namespace CreateAR.SpirePlayer.BLE
{
    public interface IBleService
    {
        void Setup(BleServiceConfiguration configuration);
        bool StartWatch(IBleWatcherDelegate @delegate);
        void StopWatch();
        void Teardown();
    }
}