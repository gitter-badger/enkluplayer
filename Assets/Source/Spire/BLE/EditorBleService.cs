namespace CreateAR.SpirePlayer.BLE
{
    public class EditorBleService : IBleService
    {
        public EditorBleService()
        {
            
        }

        public void Setup(BleServiceConfiguration configuration)
        {
            //
        }

        public bool StartWatch(IBleWatcherDelegate @delegate)
        {
            @delegate.BleDeviceInit(new BleDevice[0]);

            return true;
        }

        public void StopWatch()
        {
            //
        }

        public void Teardown()
        {
            //
        }
    }
}
