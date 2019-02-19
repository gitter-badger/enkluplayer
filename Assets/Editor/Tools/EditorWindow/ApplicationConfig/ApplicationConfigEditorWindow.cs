using System.IO;
using CreateAR.Commons.Unity.Logging;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Provides a visual editor for the application config.
    /// </summary>
    public class ApplicationConfigEditorWindow : EditorWindow
    {
        /// <summary>
        /// Watches for changes.
        /// </summary>
        private FileSystemWatcher _watcher;

        /// <summary>
        /// The combined config.
        /// </summary>
        private ApplicationConfig _config;

        /// <summary>
        /// The override config specifically.
        /// </summary>
        private ApplicationConfig _override;

        /// <summary>
        /// Flags the need for a reload.
        /// </summary>
        private bool _reload;

        /// <summary>
        /// Potential platforms.
        /// </summary>
        private static readonly string[] _Platforms =
        {
            "WebGLPlayer",
            "WSAPlayerX86"
        };

        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Tools/Application Config Editor %a")]
        private static void Open()
        {
            GetWindow<ApplicationConfigEditorWindow>();
        }

        /// <summary>
        /// Called when the window becomes active.
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Config");

            // setup loop
            UnityEditor.EditorApplication.update += Update;
            
            // create override if one doesn't exist
            var overridePath = GetOverridePath();
            if (!File.Exists(overridePath))
            {
                File.WriteAllText(
                    overridePath,
                    JsonConvert.SerializeObject(new ApplicationConfig(), new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    }));
            }

            // watch for changes
            _watcher = new FileSystemWatcher(Path.Combine(
                UnityEngine.Application.dataPath,
                "Resources"))
            {
                Filter = "*.json"
            };
            _watcher.Changed += Watcher_OnUpdated;
            _watcher.Created += Watcher_OnUpdated;
            _watcher.Deleted += Watcher_OnUpdated;
            _watcher.EnableRaisingEvents = true;

            // pull the latest config
            ReloadConfig();
        }
        
        /// <summary>
        /// Called when the window is deactivated.
        /// </summary>
        private void OnDisable()
        {
            if (null != _watcher)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            UnityEditor.EditorApplication.update -= Update;
        }

        /// <summary>
        /// Draws the window.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                var index = PlatformIndex();
                var newIndex = EditorGUILayout.Popup(
                    new GUIContent("Platform"),
                    index,
                    _Platforms);
                if (newIndex != index)
                {
                    _override.Platform = _Platforms[newIndex];

                    WriteOverride();
                }

                var level = _config.Log.ParsedLevel;
                var newLevel = (LogLevel) EditorGUILayout.EnumPopup(new GUIContent("Log Level"), level);
                if (level != newLevel)
                {
                    _override.Log.Level = newLevel.ToString();

                    WriteOverride();
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            if (_reload)
            {
                _reload = false;

                ReloadConfig();
            }
        }

        /// <summary>
        /// Finds the index into the platforms array.
        /// </summary>
        /// <returns></returns>
        private int PlatformIndex()
        {
            var platform = _config.ParsedPlatform.ToString();
            for (var i = 0; i < _Platforms.Length; i++)
            {
                if (_Platforms[i] == platform)
                {
                    return i;
                }
            }

            return 0;
        }
        
        /// <summary>
        /// Reloads the configs.
        /// </summary>
        private void ReloadConfig()
        {
            // reload
            _config = ApplicationConfigCompositor.Reload();
            _override = JsonConvert.DeserializeObject<ApplicationConfig>(
                File.ReadAllText(GetOverridePath()));
            
            Repaint();
        }

        /// <summary>
        /// Writes the override config.
        /// </summary>
        private void WriteOverride()
        {
            File.WriteAllText(
                GetOverridePath(),
                JsonConvert.SerializeObject(_override, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                }));
        }

        /// <summary>
        /// Called when a json file has been updated.
        /// </summary>
        private void Watcher_OnUpdated(
            object sender,
            FileSystemEventArgs @event)
        {
            _reload = true;
        }
        
        /// <summary>
        /// Retrieves the path to the override config.
        /// </summary>
        /// <returns></returns>
        private static string GetOverridePath()
        {
            return Path.Combine(
                UnityEngine.Application.dataPath,
                "Resources/ApplicationConfig.Override.json");
        }
    }
}