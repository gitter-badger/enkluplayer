using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using Newtonsoft.Json;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class CrashService
    {
        private const int INTERVAL_MS = 1000;
        private readonly string _lock = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

        private readonly ApplicationConfig _config;
        private readonly ApiController _api;
        private readonly string _bootLockPath;
        private readonly string _shutdownLockPath;

        /// <summary>
        /// The timer for writing to disk.
        /// </summary>
        private Timer _timer;

        public CrashService(ApplicationConfig config, ApiController api)
        {
            _config = config;
            _api = api;
            _bootLockPath = Path("Boot.lock");
            _shutdownLockPath = Path("Shutdown.lock");

            // add special crash logging for UWP
#if NETFX_CORE
            UwpCrashLogger.Initialize();
#endif
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

            // send crash logs if necessary
            if (isCrashDetected)
            {
                SendCrashLog();
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
                File.WriteAllText(path, _lock);
            }
            catch(Exception exception)
            {
                Log.Error(this, "Could not write lock: {0} : {1}.",
                    path,
                    exception);
            }
        }

        private void SendCrashLog()
        {
            var dump = BuildCrashDump();
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

                        // TODO: write dump to disk
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Fatal(this, "Could not send crash report: {0}", exception);

                    // TODO: write dump to disk
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
            // TODO: battery
            builder.Append("\n\n");
            
            // application config
            builder.Append("### ApplicationConfig\n\n");
            builder.Append(JsonConvert.SerializeObject(_config, Formatting.Indented));
            builder.Append("\n\n");

            // TODO: memory
            builder.Append("### Memory\n\n");
            builder.Append("\n\n");

            // TODO: anchors
            builder.Append("### Anchors\n\n");
            builder.Append("\n\n");

            // TODO: camera
            builder.Append("### Camera\n\n");
            builder.Append("\n\n");

            // TODO: experience
            builder.Append("### Experience\n\n");
            builder.Append("\n\n");

            // log history
            builder.Append("### Logs\n\n");
            builder.Append("\n\n");

            return builder.ToString();
        }

        private void StartDiagnosticsInterval()
        {
            // start interval that every n ms writes current data to disk
            _timer = new Timer(INTERVAL_MS);
            _timer.Elapsed += Timer_OnElapsed;
            _timer.Start();
        }

        private void Timer_OnElapsed(
            object sender,
            ElapsedEventArgs eventArgs)
        {
            // TODO: write to disk
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
                try
                {
                    File.Delete(paths[i]);
                }
                catch
                {
                    // ignored
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