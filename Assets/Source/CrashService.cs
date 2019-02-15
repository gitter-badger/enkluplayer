using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using Newtonsoft.Json;
using UnityEngine;
using Timer = System.Timers.Timer;

namespace CreateAR.EnkluPlayer
{
    public class CrashService
    {
        private const int INTERVAL_MS = 1000;
        private readonly string _lock = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

        private readonly ApplicationConfig _config;
        private readonly ApiController _api;
        private readonly RuntimeStats _stats;
        private readonly string _bootLockPath;
        private readonly string _shutdownLockPath;
        private readonly string _statsPath;

        /// <summary>
        /// The timer for writing to disk.
        /// </summary>
        private Timer _timer;

        private string _queuedDump;
        
        public CrashService(
            IMessageRouter messages,
            ApplicationConfig config,
            ApiController api,
            RuntimeStats stats)
        {
            _config = config;
            _api = api;
            _stats = stats;

            _bootLockPath = Path("Boot.lock");
            _shutdownLockPath = Path("Shutdown.lock");
            _statsPath = Path("Stats.log");

            // add special crash logging for UWP
#if NETFX_CORE
            UwpCrashLogger.Initialize();
#endif

            // listen for when we can send is ready
            messages.Subscribe(MessageTypes.LOGIN_COMPLETE, _ =>
            {
                if (!string.IsNullOrEmpty(_queuedDump))
                {
                    SendDump(_queuedDump);
                }
            });
        }

        public void Startup()
        {
            // webgl does nothing
            if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return;
            }

            // determine if there is a crash
            var isCrashDetected = IsCrashDetected(_bootLockPath, _shutdownLockPath);

            // delete locks
            Delete(_bootLockPath, _shutdownLockPath);

            // write new boot lock
            WriteLock(_bootLockPath);

            // generate crash log if last session crashed 
            if (isCrashDetected)
            {
                _queuedDump = BuildCrashDump();
            }
            
            // start interval
            StartDiagnosticsInterval();            
        }
        
        public void Shutdown()
        {
            // webgl does nothing
            if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return;
            }

            // kill timer
            _timer.Stop();

            // write shutdown lock
            WriteLock(_shutdownLockPath);
        }

        private void WriteLock(string path)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

                File.WriteAllText(path, _lock);
            }
            catch(Exception exception)
            {
                Log.Error(this, "Could not write lock: {0} : {1}.",
                    path,
                    exception);
            }
        }
        
        private void SendDump(string dump)
        {
            _api
                .Utilities
                .SendEmail(new Request
                {
                    EmailAddress = _config.Debug.DumpEmail,
                    Body = dump,
                    FirstName = "",
                    Subject = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                })
                .OnSuccess(res =>
                {
                    if (res.Payload.Success)
                    {
                        Log.Info(this, "Successfully sent crash report.");
                    }
                    else
                    {
                        Log.Fatal(this, "Could not send crash report : {0}.", res.Payload.Error);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Fatal(this, "Could not send crash report: {0}", exception);
                });
        }

        private string BuildCrashDump()
        {
            var builder = new StringBuilder();
            
            // header
            builder.Append("### Crash\n\n");
            builder.AppendFormat("Time: {0:MM/dd/yyyy - HH:mm:ss}\n", DateTime.Now);
            builder.Append("\n\n");

            // system
            builder.Append("### Device\n\n");
            builder.AppendFormat("Id: {0}\n", SystemInfo.deviceUniqueIdentifier);
            builder.AppendFormat("Name: {0}\n", SystemInfo.deviceName);
            builder.AppendFormat("Model: {0}\n", SystemInfo.deviceModel);
            builder.AppendFormat("Platform: {0}\n", UnityEngine.Application.platform.ToString());
            builder.AppendFormat("Battery: {0:0.00}\n", _stats.Device.Battery);
            builder.Append("\n\n");
            
            // application config
            builder.Append("### ApplicationConfig\n\n");
            builder.Append(JsonConvert.SerializeObject(_config, Formatting.Indented));
            builder.Append("\n\n");

            // load stats
            RuntimeStats stats = null;
            if (File.Exists(_statsPath))
            {
                var txt = File.ReadAllText(_statsPath);
                try
                {
                    stats = JsonConvert.DeserializeObject<RuntimeStats>(txt);
                }
                catch (Exception exception)
                {
                    Log.Error(this, "Could not read RuntimeStats: {0}", exception);
                }
            }

            BuildRuntimeStatsDump(builder, stats);
            
            // TODO: load logs

            return builder.ToString();
        }

        private void BuildRuntimeStatsDump(StringBuilder builder, RuntimeStats stats)
        {
            // no stats
            if (null == stats)
            {
                return;
            }
            
            // general
            builder.Append("### General\n\n");
            builder.Append("Uptime: " + Time.realtimeSinceStartup);
            builder.Append("\n\n");

            // memory
            builder.Append("### Memory\n\n");
            builder.AppendFormat("Allocated: {0:0.00} b\n", _stats.Device.AllocatedMemory);
            builder.AppendFormat("Available: {0:0.00} b\n", _stats.Device.AvailableMemory);
            builder.AppendFormat("Mono: {0:0.00} b\n", _stats.Device.MonoMemory);
            builder.AppendFormat("Reserved: {0:0.00} b\n", _stats.Device.ReservedMemory);
            builder.AppendFormat("Gpu: {0:0.00} b\n", _stats.Device.GpuMemory);
            builder.AppendFormat("Graphics Driver: {0:0.00} b\n", _stats.Device.GraphicsDriverMemory);
            builder.Append("\n\n");

            // TODO: anchors
            builder.Append("### Anchors\n\n");
            builder.Append("\n\n");

            // camera
            builder.Append("### Camera\n\n");
            builder.AppendFormat("Position: {0}\n", _stats.Camera.Position.ToString());
            builder.AppendFormat("Rotation: {0}\n", _stats.Camera.Rotation.ToString());
            builder.Append("\n\n");

            // experience
            builder.Append("### Experience\n\n");
            builder.AppendFormat("Id: {0}\n", _stats.Experience.ExperienceId);
            builder.AppendFormat("Uptime: {0}\n", ""); // TODO
            builder.Append("\n\n");

            // assets
            builder.Append("### Assets\n\n");
            builder.Append(_stats.Experience.AssetState);
            builder.Append("\n\n");

            // scripts
            builder.Append("### Scripts\n\n");
            builder.Append(_stats.Experience.ScriptState);
            builder.Append("\n\n");
        }

        private void StartDiagnosticsInterval()
        {
            // every N ms write current data to disk
            _timer = new Timer(INTERVAL_MS);

            // this will be called on the thread pool
            _timer.Elapsed += Timer_OnElapsed;
            _timer.Start();
        }

        private void Timer_OnElapsed(
            object sender,
            ElapsedEventArgs eventArgs)
        {
            try
            {
                // TODO: do this in a faster way
                var stats = JsonConvert.SerializeObject(_stats);

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_statsPath));

                // we can do a synchronous write because this is already being
                // executed off of the main thread
                File.WriteAllText(_statsPath, stats);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not write stats: {0}.", exception);
            }
        }

        private static bool IsCrashDetected(string bootPath, string shutdownPath)
        {
            if (File.Exists(bootPath))
            {
                if (File.Exists(shutdownPath))
                {
                    return File.ReadAllText(bootPath) != File.ReadAllText(shutdownPath);
                }

                return true;
            }

            return false;
        }

        private static void Delete(params string[] paths)
        {
            for (int i = 0, len = paths.Length; i < len; i++)
            {
                var path = paths[i];
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                }
                catch (Exception exception)
                {
                    Log.Error(null,
                        "Could not delete {0} : {1}.",
                        path,
                        exception);
                }
            }
        }

        private static string Path(string fileName)
        {
            return System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                System.IO.Path.Combine(
                    "Diagnostics",
                    fileName));
        }
    }
}