using System;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Lookup table for vines.
    /// </summary>
    public class VineTable : MonoBehaviour, IVineTable
    {
        /// <summary>
        /// Queued actions that need to happen on the main thread.
        /// </summary>
        private readonly Queue<Action> _actions = new Queue<Action>();

        /// <summary>
        /// Path watcher starts in.
        /// </summary>
        private string _rootPath;

        /// <summary>
        /// References.
        /// </summary>
        public VineReference[] References;

        /// <inheritdoc />
        public VineReference Vine(string identifier)
        {
            for (int i = 0, len = References.Length; i < len; i++)
            {
                var reference = References[i];
                if (reference.Identifier == identifier)
                {
                    return reference;
                }
            }

            return null;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Awake()
        {
            Watch();
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            lock (_actions)
            {
                while (_actions.Count > 0)
                {
                    _actions.Dequeue()();
                }
            }
        }

        /// <summary>
        /// Watches for vine changes.
        /// </summary>
        private void Watch()
        {
            _rootPath = Path.Combine(UnityEngine.Application.dataPath, "");
            
#if UNITY_EDITOR
            var watcher = new FileSystemWatcher
            {
                Path = _rootPath,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.txt"
            };

            watcher.Changed += Watcher_OnChanged;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called when a vine changes, on a separate thread.
        /// </summary>
        private void Watcher_OnChanged(
            object sender,
            FileSystemEventArgs fileSystemEventArgs)
        {
            lock (_actions)
            {
                _actions.Enqueue(() =>
                {
                    var path = fileSystemEventArgs.FullPath;
                    var index = path.IndexOf("Assets");
                    var adjustedPath = path.Substring(index);

                    Log.Info(this, "{0} was updated.", adjustedPath);

                    for (var i = 0; i < References.Length; i++)
                    {
                        var reference = References[i];
                        var referencePath = UnityEditor
                            .AssetDatabase
                            .GetAssetPath(reference.Source)
                            .Replace('/', Path.DirectorySeparatorChar);

                        if (referencePath == adjustedPath)
                        {
                            UnityEditor.AssetDatabase.ImportAsset(
                                UnityEditor.AssetDatabase.GetAssetPath(reference.Source),
                                UnityEditor.ImportAssetOptions.ForceSynchronousImport);

                            reference.Updated();
                        }
                    }
                });
            }
        }
#endif
    }
}