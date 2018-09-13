using System;
using System.IO;
using CreateAR.Commons.Unity.Logging;
using UnityEditor;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Hooks for managing persistent data.
    /// </summary>
    public static class PersistentDataHooks
    {
        /// <summary>
        /// Clears all persistent data.
        /// </summary>
        [MenuItem("Tools/Persistent Data/Clear All")]
        public static void ClearAllPersistentData()
        {
            var dir = new DirectoryInfo(UnityEngine.Application.persistentDataPath);

            foreach (var file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception exception)
                {
                    Log.Error(null, "Could not delete file {0} : {1}.",
                        file.FullName,
                        exception);
                }
            }

            foreach (var directory in dir.GetDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                catch (Exception exception)
                {
                    Log.Error(null, "Could not delete directory {0} : {1}.",
                        directory.FullName,
                        exception);
                }
            }
        }
    }
}