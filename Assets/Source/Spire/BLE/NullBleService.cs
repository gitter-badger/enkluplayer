namespace CreateAR.SpirePlayer.BLE
{
    /// <summary>
    /// <c>IBleService</c> implementation that does nothing.
    /// </summary>
    public class NullBleService : IBleService
    {
        /// <inheritdoc />
        public void Setup(BleServiceConfiguration configuration)
        {
            //
        }

        /// <inheritdoc />
        public bool StartWatch(IBleWatcherDelegate @delegate)
        {
            @delegate.BleDeviceInit(new BleDevice[0]);

            return true;
        }

        /// <inheritdoc />
        public void StopWatch()
        {
            //
        }

        /// <inheritdoc />
        public void Teardown()
        {
            //
        }
    }
}