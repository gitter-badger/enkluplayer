#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.BLE
{
    public class UwpBleService : IBleService
    {
        private BluetoothLEAdvertisementWatcher _adWatcher;
        private DeviceWatcher _deviceWatcher;

        private IBleWatcherDelegate _watcherDelegate;
        private readonly List<BleDevice> _bleDevices = new List<BleDevice>();
        private bool _enumerationComplete = false;

        public UwpBleService()
        {
            
        }

        public void Setup(BleServiceConfiguration configuration)
        {
            //_watcher = DeviceInformation.CreateWatcher(@"System.Devices.Aep.ProtocolId:=""{bb7bb05e-5972-42b5-94fc76eaa7084d49}""");
            string[] requestedProperties =
            {
                "System.Devices.Aep.DeviceAddress",
                "System.Devices.Aep.SignalStrength",
                "System.Devices.AepService.Bluetooth.ServiceGuid",
                "System.Devices.Aep.Category",
                "System.Devices.AepContainer.ModelIds",
                "System.Devices.AepContainer.ModelName",
                "System.Devices.AepService.FriendlyName",
                "System.Devices.AepService.ServiceId"
            };

            // BT_Code: Example showing paired and non-paired in a single query.
            //var aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\") AND (System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#False)";
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            _adWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            _adWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));
            _adWatcher.Received += Watcher_OnReceived;

            _deviceWatcher = DeviceInformation.CreateWatcher(
                aqsAllBluetoothLEDevices,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint);

            _deviceWatcher.Added += Watcher_OnAdded;
            _deviceWatcher.Updated += Watcher_OnUpdated;
            _deviceWatcher.Removed += Watcher_OnRemoved;
            _deviceWatcher.EnumerationCompleted += Watcher_EnumerationCompleted;
        }

        private bool _found = false;
        
        private async void Watcher_OnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Address : {0}\n", args.BluetoothAddress);
            builder.AppendFormat("Signal  : {0}\n", args.RawSignalStrengthInDBm);
            builder.AppendFormat("Advertisement:\n");
            builder.AppendFormat("\tLocal Name : {0}\n", args.Advertisement.LocalName);
            builder.AppendFormat("\tServices   : {0}\n", string.Join(", ", args.Advertisement.ServiceUuids.ToArray()));
            builder.AppendFormat("\tData Sections:\n");
            foreach (var section in args.Advertisement.DataSections)
            {
                builder.AppendFormat("\t\t{0}: {1}\n", section.DataType, BitConverter.ToString(section.Data.ToArray()));
            }
            builder.AppendFormat("\tManufacturer Data:\n");
            foreach (var section in args.Advertisement.ManufacturerData)
            {
                builder.AppendFormat("\t\t{0}: {1}\n", section.CompanyId, BitConverter.ToString(section.Data.ToArray()));
            }

            if (!_found)
            {
                Log.Info(this, "Advert from device :\n{0}", builder);
            }
            else
            {
                return;
            }

            _found = true;
            
            /*var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);

            Log.Debug(this, $"Device found {device.DeviceId} : {device.DeviceAccessInformation.CurrentStatus} : {device.DeviceInformation.Name}");
            
            try
            {
                //var services = await device.GetGattServicesAsync();
            }
            catch (Exception exception)
            {
                Log.Error(this, "Exception caught : " + exception);
            }

            Log.Debug(this, "Services retrieved.");*/
        }

        public void Teardown()
        {
            StopWatch();

            _deviceWatcher.Added -= Watcher_OnAdded;
            _deviceWatcher.Updated -= Watcher_OnUpdated;
            _deviceWatcher.Removed -= Watcher_OnRemoved;
            _deviceWatcher.EnumerationCompleted -= Watcher_EnumerationCompleted;
            _deviceWatcher = null;

            _adWatcher.Received -= Watcher_OnReceived;
            _adWatcher = null;
        }

        public bool StartWatch(IBleWatcherDelegate @delegate)
        {
            if (_deviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                return false;
            }

            _watcherDelegate = @delegate;
            _adWatcher.Start();
            //_deviceWatcher.Start();
            
            return true;
        }
        
        public void StopWatch()
        {
            if (_deviceWatcher.Status != DeviceWatcherStatus.Started)
            {
                return;
            }

            _adWatcher.Stop();
            _watcherDelegate = null;
            _enumerationComplete = false;
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

        private async void Watcher_OnAdded(
            DeviceWatcher sender,
            DeviceInformation info)
        {
            var device = new BleDevice
            {
                Id = info.Id,
                Name = info.Name,
                Props = ToProps(info.Properties)
            };
            _bleDevices.Add(device);

            if ((string) info.Properties["System.Devices.Aep.DeviceAddress"] == "00:15:83:00:b6:cb")
            {
                Log.Info(this, "Device added :\n{0}", device);

                if (info.Pairing.CanPair)
                {
                    Log.Info(this, "Device can pair.");

                    if (!info.Pairing.IsPaired)
                    {
                        Log.Info(this, "Devive is not paired yet. Protection level : {0}.",
                            info.Pairing.ProtectionLevel);

                        info.Pairing.Custom.PairingRequested += (pairing, args) =>
                        {
                            args.Accept();
                        };

                        var result = await info.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly);

                        Log.Info(this, "Pairing attempted. Result : {0} - {1}.",
                            result.Status,
                            result.ProtectionLevelUsed);
                    }
                }
            }

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