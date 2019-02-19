#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using CreateAR.Commons.Unity.Logging;
using Newtonsoft.Json;
using Windows.Networking.Sockets;
using Windows.Security.Credentials;
using Windows.Storage.Streams;

namespace CreateAR.EnkluPlayer
{
    public class HoloLensDeviceStats : IDeviceStats
    {
        private DebugConfig _config;
        private RuntimeStats _stats;
        
        public HoloLensDeviceStats(ApplicationConfig appConfig, RuntimeStats stats)
        {
            _config = appConfig.Debug;
            _stats = stats;
            
            PollStats();
        }

        public async void PollStats()
        {
            Log.Info(this, "Connecting to HoloLens dev portal...");

            var bytes = new byte[1024];
            var buffer = new ArraySegment<byte>(bytes);

            var socket = new MessageWebSocket();
            socket.Control.MessageType = SocketMessageType.Utf8;
            socket.Control.ServerCredential = new PasswordCredential {
                UserName = "", // TODO: Connect user/pass to config
                Password = ""
            };

            var stats = _stats.Device;
            
            socket.MessageReceived += (sender, args) => {
                using (DataReader dataReader = args.GetDataReader())
                {
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);

                    var deviceStats = JsonConvert.DeserializeObject<DeviceStats>(message);

                    stats.CpuLoad = deviceStats.CpuLoad;
                    stats.AvailableMemory =
                        RuntimeStats.BytesToMb(deviceStats.AvailablePages * deviceStats.PageSize);

                    if (deviceStats.GPUData.AvailableAdapters.Count > 0)
                    {
                        var gpuData = deviceStats.GPUData.AvailableAdapters[0];

                        stats.GpuLoad = (float) gpuData.EnginesUtilization[0];
                        stats.GpuDedicatedMemory = gpuData.DedicatedMemory;
                        stats.GpuDedicatedMemoryUsed = gpuData.DedicatedMemoryUsed;
                        stats.GpuSystemMemory = gpuData.SystemMemory;
                        stats.GpuSystemMemoryUsed = gpuData.SystemMemoryUsed;
                    }

                    stats.NetworkIn = RuntimeStats.BytesToMb(deviceStats.NetworkingData.NetworkInBytes);
                    stats.NetworkOut = RuntimeStats.BytesToMb(deviceStats.NetworkingData.NetworkOutBytes);

                    stats.IoRead = RuntimeStats.BytesToMb(deviceStats.IOReadSpeed);
                    stats.IoWrite = RuntimeStats.BytesToMb(deviceStats.IOWriteSpeed);
                    
                    Log.Info(this, "Cpu: {0}  Gpu: {1}  Free: {2}  Ded: {3}  DedUsed: {4}  Sys: {5}  SysUsed: {6}  NetIn: {7}  NetOut: {8}  IoRead: {9}  IoWrite: {10}",
                        stats.CpuLoad, stats.GpuLoad, stats.AvailableMemory, stats.GpuDedicatedMemory, stats.GpuDedicatedMemoryUsed,
                        stats.GpuSystemMemory, stats.GpuSystemMemoryUsed, stats.NetworkIn, stats.NetworkOut, stats.IoRead, stats.IoWrite);
                }
            };

            socket.Closed += (sender, args) => {
                
            };

            await socket.ConnectAsync(new Uri("ws://127.0.0.1:10080/api/resourcemanager/systemperf"));
            Log.Info(this, "Connected.");
        }

        public struct DeviceStats
        {
            public int AvailablePages { get; set; }
            public int CommitLimit { get; set; }
            public int CommittedPages { get; set; }
            public int CpuLoad { get; set; }
            public int IOOtherSpeed { get; set; }
            public int IOReadSpeed { get; set; }
            public int IOWriteSpeed { get; set; }
            public int NonPagedPoolPages { get; set; }
            public int PageSize { get; set; }
            public int PagedPoolPages { get; set; }
            public int TotalInstalledInKb { get; set; }
            public int TotalPages { get; set; }
            public GPUData GPUData { get; set; }
            public NetworkingData NetworkingData { get; set; }
        }
        
        public struct AvailableAdapter
        {
            public int DedicatedMemory { get; set; }
            public int DedicatedMemoryUsed { get; set; }
            public string Description { get; set; }
            public int SystemMemory { get; set; }
            public int SystemMemoryUsed { get; set; }
            public List<double> EnginesUtilization { get; set; }
        }
        
        public struct GPUData
        {
            public List<AvailableAdapter> AvailableAdapters { get; set; }
        }

        public struct NetworkingData
        {
            public double NetworkInBytes { get; set; }
            public double NetworkOutBytes { get; set; }
        }

    }
}

#endif