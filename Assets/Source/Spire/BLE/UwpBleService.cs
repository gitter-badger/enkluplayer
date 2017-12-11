#if NETFX_CORE

using System.Collections.Generic;
using Windows.Devices.Enumeration;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer.BLE
{
    public class UwpBleService : IBleService
    {
        private const string AQS_FILTER = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        private readonly string[] REQUESTED_PROPERTIES = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        private readonly DeviceWatcher _watcher;

        private IBleWatcherDelegate _watcherDelegate;
        private readonly List<BleDevice> _bleDevices = new List<BleDevice>();
        private bool _enumerationComplete = false;

        public UwpBleService()
        {
            _watcher = DeviceInformation.CreateWatcher();

            _watcher.Added += Watcher_OnAdded;
            _watcher.Updated += Watcher_OnUpdated;
            _watcher.Removed += Watcher_OnRemoved;
            _watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
        }

        public void Setup(BleServiceConfiguration configuration)
        {
            
        }

        public bool StartWatch(IBleWatcherDelegate @delegate)
        {
            if (_watcher.Status == DeviceWatcherStatus.Started)
            {
                return false;
            }

            _watcherDelegate = @delegate;
            _watcher.Start();

            return true;
        }

        public void StopWatch()
        {
            if (_watcher.Status != DeviceWatcherStatus.Started)
            {
                return;
            }

            _watcher.Stop();
            _watcherDelegate = null;
            _enumerationComplete = false;
        }

        public void Teardown()
        {
            StopWatch();
        }

        private void Watcher_EnumerationCompleted(
            DeviceWatcher sender,
            object args)
        {
            _enumerationComplete = true;

            Log.Info(this, "UWPBLE Enum complete.");

            _watcherDelegate.BleDeviceInit(_bleDevices.ToArray());
        }

        private void Watcher_OnRemoved(
            DeviceWatcher sender,
            DeviceInformationUpdate @event)
        {
            var id = @event.Id;
            var device = Device(id);
            if (null == device)
            {
                Log.Error(this,
                    "BLE remove event for device we don't know about : {0}.",
                    @event);
                return;
            }

            _bleDevices.Remove(device);

            if (_enumerationComplete)
            {
                _watcherDelegate.BleDeviceRemoved(@event.Id);
            }
        }

        private void Watcher_OnUpdated(
            DeviceWatcher sender,
            DeviceInformationUpdate @event)
        {
            var id = @event.Id;
            var device = Device(id);
            if (null == device)
            {
                Log.Error(this,
                    "BLE update event for device we don't know about : {0}.",
                    @event);
                return;
            }

            // TODO: update!

            if (_enumerationComplete)
            {
                _watcherDelegate.BleDeviceUpdated(id);
            }
        }

        private void Watcher_OnAdded(
            DeviceWatcher sender,
            DeviceInformation @event)
        {
            var device = new BleDevice
            {
                Id = @event.Id,
                Name = @event.Name,
                Props = ToProps(@event.Properties)
            };
            _bleDevices.Add(device);

            if (_enumerationComplete)
            {
                _watcherDelegate.BleDeviceAdded(device);
            }
        }

        private BleDevice Device(string id)
        {
            for (int i = 0, len = _bleDevices.Count; i < len; i++)
            {
                var device = _bleDevices[i];
                if (device.Id == id)
                {
                    return device;
                }
            }

            return null;
        }

        private Dictionary<string, object> ToProps(IReadOnlyDictionary<string, object> props)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var pair in props)
            {
                dictionary[pair.Key] = pair.Value;
            }

            return dictionary;
        }
    }
}

#endif