using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.BLE;

namespace CreateAR.SpirePlayer
{
    public class BleSearchApplicationState : IState, IBleWatcherDelegate
    {
        private readonly IBleService _ble;
        
        private readonly List<BleDevice> _availableDevices = new List<BleDevice>();

        public BleSearchApplicationState(
            IBleService ble)
        {
            _ble = ble;
        }

        public void Enter(object context)
        {
            Log.Info(this, "Starting bluetooth service.");

            _ble.StartWatch(this);
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            _ble.StopWatch();

            Log.Info(this, "Stopped bluetooth service.");
        }

        public void BleDeviceInit(BleDevice[] devices)
        {
            _availableDevices.AddRange(devices);

            LogDevices();
        }

        public void BleDeviceAdded(BleDevice device)
        {
            _availableDevices.Add(device);

            Log.Info(this, "BLE device added : {0}.", device);
        }

        public void BleDeviceUpdated(string id)
        {
            var device = Device(id);

            Log.Info(this, "BLE device updated : {0}.", device);
        }

        public void BleDeviceRemoved(string id)
        {
            var device = Device(id);

            _availableDevices.Remove(device);

            Log.Info(this, "BLE device removed : {0}.", device);
        }

        private BleDevice Device(string id)
        {
            for (int i = 0, len = _availableDevices.Count; i < len; i++)
            {
                var device = _availableDevices[i];
                if (device.Id == id)
                {
                    return device;
                }
            }

            return null;
        }

        private void LogDevices()
        {
            Log.Info(this, "Available devices:");
            foreach (var device in _availableDevices)
            {
                Log.Info(this, "\t{0}", device);
            }
        }
    }
}
