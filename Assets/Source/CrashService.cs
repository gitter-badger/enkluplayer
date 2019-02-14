using System;
using System.Globalization;
using System.IO;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    public class CrashService
    {
        private readonly string _lock = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

        private readonly string _bootLockPath;
        private readonly string _shutdownLockPath;

        public CrashService()
        {
            _bootLockPath = Path("Boot.lock");
            _shutdownLockPath = Path("Shutdown.lock");
        }

        public void Startup()
        {
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
            // TODO: build crash logs

            // TODO: send crash log
        }

        private void StartDiagnosticsInterval()
        {
            // TODO: start interval that every n ms writes current data to disk
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