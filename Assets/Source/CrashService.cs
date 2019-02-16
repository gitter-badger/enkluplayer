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
    /// <summary>
    /// Service that provides detailed crash logs.
    /// </summary>
    public class CrashService
    {
        /// <summary>
        /// The interval at which we write data.
        /// </summary>
        private const int INTERVAL_MS = 500;

        /// <summary>
        /// Acts as a GUID for writing lock files.
        /// </summary>
        private readonly string _lock = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly ApplicationConfig _config;
        private readonly ApiController _api;
        private readonly RuntimeStats _stats;

        /// <summary>
        /// Useful paths.
        /// </summary>
        private readonly string _bootLockPath;
        private readonly string _shutdownLockPath;
        private readonly string _statsPath;

        /// <summary>
        /// The timer for writing to disk.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// A dump is created at startup, then queued until we are logged in and can send it.
        /// </summary>
        private string _queuedDump;
        
        /// <summary>
        /// Constructor.
        /// </summary>
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

        /// <summary>
        /// Starts the crash service.
        /// </summary>
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
        
        /// <summary>
        /// Exits the service gracefully. This should be called when the application is shutting down.
        /// </summary>
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

        /// <summary>
        /// Writes the lock to a path.
        /// </summary>
        /// <param name="path">The path to write to.</param>
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
        
        /// <summary>
        /// Sends a dump to the configured dump email.
        /// </summary>
        /// <param name="dump">The dump.</param>
        private void SendDump(string dump)
        {
            if (UnityEngine.Application.isEditor)
            {
                return;
            }

            Log.Debug(this, "Sending dump:\n{0}", dump);

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

        /// <summary>
        /// Builds a crash dump with all available info.
        /// </summary>
        /// <returns></returns>
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
            BuildLogDump(builder);

            return builder.ToString();
        }
        
        /// <summary>
        /// Builds a dump of just the runtime stats.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="stats">The stats to dump.</param>
        private void BuildRuntimeStatsDump(StringBuilder builder, RuntimeStats stats)
        {
            // no stats
            if (null == stats)
            {
                return;
            }
            
            // general
            builder.Append("### General\n\n");
            builder.AppendFormat("Uptime: {0:0.00}\n", stats.Uptime);
            builder.AppendFormat("Battery: {0:0.00}\n", stats.Device.Battery);
            builder.Append("\n\n");

            // memory
            builder.Append("### Memory\n\n");
            builder.AppendFormat("Allocated: {0:0.00} b\n", stats.Device.AllocatedMemory);
            builder.AppendFormat("Available: {0:0.00} b\n", stats.Device.AvailableMemory);
            builder.AppendFormat("Mono: {0:0.00} b\n", stats.Device.MonoMemory);
            builder.AppendFormat("Reserved: {0:0.00} b\n", stats.Device.ReservedMemory);
            builder.AppendFormat("Gpu: {0:0.00} b\n", stats.Device.GpuMemory);
            builder.AppendFormat("Graphics Driver: {0:0.00} b\n", stats.Device.GraphicsDriverMemory);
            builder.Append("\n\n");

            // anchors
            builder.Append("### Anchors\n\n");
            for (int i = 0, len = _stats.Anchors.States.Length; i < len; i++)
            {
                var state = stats.Anchors.States[i];
                builder.AppendFormat("Anchor: {0}\n", state.Id);
                builder.AppendFormat("Status: {0}\n", state.Status);
                builder.AppendFormat("Unlocated Duration: {0}\n\n", state.TimeUnlocated);
            }
            builder.Append("\n");

            // camera
            builder.Append("### Camera\n\n");
            builder.AppendFormat("Position: {0}\n", stats.Camera.Position.ToString());
            builder.AppendFormat("Rotation: {0}\n", stats.Camera.Rotation.ToString());
            builder.AppendFormat("Relative To: {0}\n", stats.Camera.AnchorRelativeTo);
            builder.Append("\n\n");

            // experience
            builder.Append("### Experience\n\n");
            builder.AppendFormat("Id: {0}\n", stats.Experience.ExperienceId);
            builder.AppendFormat("Uptime: {0}\n", stats.Uptime);
            builder.Append("\n\n");

            // assets
            builder.Append("### Assets\n\n");
            builder.AppendFormat("Queue Length: {0}\n", stats.Experience.AssetState.QueueLength);
            builder.AppendFormat("Error: {0}", stats.Experience.AssetState.Errors);
            builder.Append("\n\n");

            // scripts
            builder.Append("### Scripts\n\n");
            builder.AppendFormat("Queue Length: {0}\n", stats.Experience.ScriptState.QueueLength);
            builder.AppendFormat("Errors: {0}\n", stats.Experience.ScriptState.Errors);
            builder.Append("\n\n");
        }

        /// <summary>
        /// Appends last log file.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildLogDump(StringBuilder builder)
        {
            var logPath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Application.previous.log");
            if (File.Exists(logPath))
            {
                const int maxSize = 4096;
                var logs = File.ReadAllText(logPath);

                // trim logs
                if (logs.Length > maxSize)
                {
                    logs = logs.Substring(logs.Length - maxSize);
                }

                builder.Append(logs);
            }
        }

        /// <summary>
        /// Starts the interval to write to disk.
        /// </summary>
        private void StartDiagnosticsInterval()
        {
            // every N ms write current data to disk
            _timer = new Timer(INTERVAL_MS);

            // this will be called on the thread pool
            _timer.Elapsed += Timer_OnElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Called when the write interval elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
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

        /// <summary>
        /// True iff a crash has been detected.
        /// </summary>
        /// <param name="bootPath">The path to the boot lock.</param>
        /// <param name="shutdownPath">The path to the shutdown lock.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes all files.
        /// </summary>
        /// <param name="paths">The absolute path to the files to delete.</param>
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

        /// <summary>
        /// Generates a path to a specific file inside the diagnostics folder.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns></returns>
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
