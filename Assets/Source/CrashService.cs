using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using Newtonsoft.Json;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        private readonly string _persistentDataPath;
        private readonly string _bootLockPath;
        private readonly string _shutdownLockPath;
        private readonly string _statsPath;

        /// <summary>
        /// Statically created device information.
        /// </summary>
        private readonly string _deviceInfo;

        /// <summary>
        /// The timer for writing to disk.
        /// </summary>
        private Timer _timer;
        
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

            _persistentDataPath = UnityEngine.Application.persistentDataPath;
            _bootLockPath = GetPath("Boot.lock");
            _shutdownLockPath = GetPath("Shutdown.lock");
            _statsPath = GetPath("Stats.log");

            var builder = new StringBuilder();
            builder.AppendFormat("Id: {0}\n", SystemInfo.deviceUniqueIdentifier);
            builder.AppendFormat("Name: {0}\n", SystemInfo.deviceName);
            builder.AppendFormat("Model: {0}\n", SystemInfo.deviceModel);
            builder.AppendFormat("Platform: {0}\n", UnityEngine.Application.platform.ToString());
            _deviceInfo = builder.ToString();

            // add special crash logging for UWP
#if NETFX_CORE || (!UNITY_EDITOR && UNITY_WSA)
            UwpCrashLogger.Initialize();

            // handle shutdown
            Windows.ApplicationModel.Core.CoreApplication.Suspending += (sender, o) => Shutdown();
            Windows.ApplicationModel.Core.CoreApplication.Resuming += (sender, o) => Startup();
#endif

            // listen for when we can send is ready
            messages.Subscribe(MessageTypes.LOGIN_COMPLETE, _ => SendQueuedDumps());
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

            Log.Info(this, "Startup.");

            // determine if there is a crash
            var isCrashDetected = IsCrashDetected(_bootLockPath, _shutdownLockPath);

            // delete locks
            Delete(_bootLockPath, _shutdownLockPath);

            // write new boot lock
            WriteLock(_bootLockPath);

            // generate crash log if last session crashed 
            if (isCrashDetected)
            {
                Log.Info(this, "Crash detected, writing to disk.");

                // write to disk
                var dump = BuildCrashDump();
                var path = GetUniqueDumpPath();
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    File.WriteAllText(
                        path,
                        dump);
                }
                catch (Exception ex)
                {
                    Log.Fatal(this,
                        "Could not write dump to disk: {0}\n\n{1}",
                        ex, dump);
                }
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

            Log.Info(this, "Shutdown.");

            // kill timer
            if (null != _timer)
            {
                _timer.Dispose();
            }

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
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, _lock);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not write lock: {0} : {1}.",
                    path,
                    exception);
            }
        }

        /// <summary>
        /// Sends all queued dumps and deletes them once they are verified sent.
        /// </summary>
        private void SendQueuedDumps()
        {
            Log.Info(this, "Attempting to send queued dumps.");

            var path = GetDumpFolder();
            string[] dumps;
            try
            {
                dumps = Directory.GetFiles(path);
            }
            catch
            {
                // directory may not exist yet
                return;
            }

            Log.Info(this, "Found {0} crash dumps.", dumps.Length);

            for (int i = 0, len = dumps.Length; i < len; i++)
            {
                var dumpPath = dumps[i];

                string dump;
                try
                {
                    dump = File.ReadAllText(dumps[i]);
                }
                catch (Exception ex)
                {
                    Log.Error(this,
                        "Could not read dump '{0}' : {1}",
                        dumpPath,
                        ex);

                    // try again next time

                    continue;
                }

                Log.Debug(this, "Sending dump:\n{0}", dumpPath);

                SendDump(dump)
                    .OnSuccess(_ =>
                    {
                        Log.Debug(this, "Successfully sent dump.");

                        // delete dump
                        try
                        {
                            File.Delete(dumpPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "Could not delete dump '{0}': {1}",
                                dumpPath,
                                ex);
                        }
                    })
                    // send through logs, this will allow retry later
                    .OnFailure(ex => Log.Fatal(this, "Could not send dump: {0}.", ex));
            }
        }

        /// <summary>
        /// Sends a dump to the configured dump email.
        /// </summary>
        /// <param name="dump">The dump.</param>
        private IAsyncToken<Void> SendDump(string dump)
        {
            // if we're in the editor, don't actually send the email.
            if (UnityEngine.Application.isEditor)
            {
                return new AsyncToken<Void>(Void.Instance);
            }

            var token = new AsyncToken<Void>();

            _api
                .Utilities
                .SendEmail(new Request
                {
                    EmailAddress = _config.Debug.DumpEmail,
                    Body = dump,
                    FirstName = "",
                    Subject = "Crash: " + DateTime.Now.ToString(CultureInfo.InvariantCulture)
                })
                .OnSuccess(res =>
                {
                    if (null == res.Payload)
                    {
                        token.Fail(new Exception("Could not deserialize payload."));
                    }
                    else if (res.Payload.Success)
                    {
                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(res.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
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
            builder.Append(_deviceInfo);
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
            builder.AppendFormat("Next: {0}\n", stats.Experience.AssetState.NextLoad);
            builder.AppendFormat("Error: {0}", stats.Experience.AssetState.Errors);
            builder.Append("\n\n");

            // scripts
            builder.Append("### Scripts\n\n");
            builder.AppendFormat("Queue Length: {0}\n", stats.Experience.ScriptState.QueueLength);
            builder.AppendFormat("Errors: {0}\n", stats.Experience.ScriptState.Errors);
            builder.Append("\n\n");
        }
        
        /// <summary>
        /// Starts the interval to write to disk.
        /// </summary>
        private void StartDiagnosticsInterval()
        {
            // make sure directory exists
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_statsPath));
            }
            catch (Exception ex)
            {
                Log.Fatal(this,
                    "Could not create directory to write diagnostics: {0}",
                    ex);

                // do not start interval, as we can't write stats
                return;
            }

            // every N ms write current data to disk
            _timer = new Timer(_ =>
                {
                    try
                    {
                        // TODO: do this in a faster way
                        var stats = JsonConvert.SerializeObject(_stats);

                        // we can do a synchronous write because this is already being
                        // executed off of the main thread
                        File.WriteAllText(_statsPath, stats);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(this, "Could not write stats: {0}.", exception);
                    }
                },
                Void.Instance,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromMilliseconds(INTERVAL_MS));
        }

        /// <summary>
        /// Appends last log file.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void BuildLogDump(StringBuilder builder)
        {
            var logPath = Path.Combine(
                _persistentDataPath,
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
        private string GetPath(string fileName)
        {
            return Path.Combine(
                _persistentDataPath,
                Path.Combine(
                    "Diagnostics",
                    fileName));
        }

        /// <summary>
        /// Generates a unique path for a dump.
        /// </summary>
        private string GetUniqueDumpPath()
        {
            return Path.Combine(
                GetDumpFolder(),
                DateTime.Now.Ticks + ".dump");
        }
        
        /// <summary>
        /// Retrieves the dump folder.
        /// </summary>
        /// <returns></returns>
        private string GetDumpFolder()
        {
            return Path.Combine(
                _persistentDataPath,
                Path.Combine(
                    "Diagnostics",
                    "Dumps"));
        }
    }
}
