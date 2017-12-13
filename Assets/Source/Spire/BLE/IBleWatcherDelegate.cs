namespace CreateAR.SpirePlayer.BLE
{
    public interface IBleWatcherDelegate
    {
        void BleDeviceInit(BleDevice[] devices);
        void BleDeviceAdded(BleDevice device);
        void BleDeviceUpdated(string id);
        void BleDeviceRemoved(string id);
    }
}