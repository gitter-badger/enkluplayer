using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    public class TrellisSettings
    {
        public string Email;
        public string Password;
        public string Token;
    }

    public class TrellisSettingsWindow : EditorWindow
    {
        private static readonly JsonSerializer _serializer = new JsonSerializer();
        
        private TrellisSettings _settings;

        private void OnEnable()
        {
            titleContent = new GUIContent("Trellis");

            LoadSettings();
        }
        
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical("box");
                {
                    _settings.Email = EditorGUILayout.TextField("Email", _settings.Email);
                    _settings.Password = EditorGUILayout.TextField("Password", _settings.Password);
                    
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Save"))
                        {
                            Save(_settings);
                        }
                        
                        GUILayout.FlexibleSpace();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private void GetToken()
        {
            const string popupTitle = "Connecting to Trellis...";
            
            EditorUtility.DisplayProgressBar(
                popupTitle,
                "Attempting to sign in.",
                0f);
            
            
        }

        private void Save(TrellisSettings settings)
        {
            byte[] bytes;
            _serializer.Serialize(settings, out bytes);

            File.WriteAllText(
                GetSettingsPath(),
                Encoding.UTF8.GetString(bytes));
        }

        private void LoadSettings()
        {
            // load/create settings object
            var path = GetSettingsPath();
            if (!File.Exists(path))
            {
                Save(new TrellisSettings());
            }

            var json = File.ReadAllText(path);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            object @object;
            _serializer.Deserialize(
                typeof(TrellisSettings),
                ref jsonBytes,
                out @object);

            _settings = (TrellisSettings) @object;
        }

        private static string GetSettingsPath()
        {
            var path = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Trellis.config");
            return path;
        }
    }
}