using System.IO;
using UnityEditor;

namespace CreateAR.SpirePlayer
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
                file.Delete();
            }

            foreach (var directory in dir.GetDirectories())
            {
                directory.Delete(true);
            }
        }
    }
}