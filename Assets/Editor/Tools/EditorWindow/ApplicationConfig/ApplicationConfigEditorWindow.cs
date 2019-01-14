using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    public class ApplicationConfigEditorWindow : EditorWindow
    {
        private FileSystemWatcher _watcher;
        private ApplicationConfig _config;
        private ApplicationConfig _override;

        private bool _reload;

        private static readonly string[] _Platforms =
        {
            "WebGLPlayer",
            "WSAPlayerX86"
        };

        [MenuItem("Tools/Application Config Editor %a")]
        private static void Open()
        {
            GetWindow<ApplicationConfigEditorWindow>();
        }

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
                    JsonConvert.SerializeObject(new ApplicationConfig()));
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
        
        private void OnDisable()
        {
            if (null != _watcher)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

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
            }
            GUILayout.EndVertical();
        }

        private void Update()
        {
            if (_reload)
            {
                _reload = false;

                ReloadConfig();
            }
        }

        private int PlatformIndex()
        {
            var platform = _config.ParsedPlatform;
            for (var i = 0; i < _Platforms.Length; i++)
            {
                if (_Platforms[i] == platform.ToString())
                {
                    return i;
                }
            }

            return 0;
        }

        private void ReloadConfig()
        {
            // reload
            _config = ApplicationConfigCompositor.Reload();
            _override = JsonConvert.DeserializeObject<ApplicationConfig>(
                File.ReadAllText(GetOverridePath()));
            
            Repaint();
        }

        private void WriteOverride()
        {
            File.WriteAllText(
                GetOverridePath(),
                JsonConvert.SerializeObject(_override, Formatting.Indented));
        }

        private void Watcher_OnUpdated(
            object sender,
            FileSystemEventArgs @event)
        {
            _reload = true;
        }

        private static string GetOverridePath()
        {
            return Path.Combine(
                UnityEngine.Application.dataPath,
                "Resources/ApplicationConfig.Override.json");
        }
    }
}