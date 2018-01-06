using System;
using System.IO;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.EmailSignIn;
using UnityEditor;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Window for setting up Trellis integration.
    /// </summary>
    public class TrellisSettingsWindow : EditorWindow
    {
        /// <summary>
        /// Serialize settings.
        /// </summary>
        private static readonly JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Configuration for all environments.
        /// </summary>
        private EnvironmentConfig _environments;
        
        /// <summary>
        /// User settings.
        /// </summary>
        private UserEnvironmentSettings _settings;

        /// <summary>
        /// Message to display.
        /// </summary>
        private string _logMessage;
        
        /// <summary>
        /// Title for signin.
        /// </summary>
        private const string SIGNIN_PROGRESS_TITLE = "Connecting to Trellis...";

        /// <summary>
        /// Opens window.
        /// </summary>
        [MenuItem("Tools/Settings/Trellis")]
        private static void Open()
        {
            GetWindow<TrellisSettingsWindow>();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("Trellis");

            LoadEnvironments();
            LoadCredentials();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            EditorUtility.ClearProgressBar();
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                DrawEnvironmentSelection();
                DrawConnectForm();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws environment selection panel.
        /// </summary>
        private void DrawEnvironmentSelection()
        {
            EditorGUIUtility.labelWidth = 100;
            
            var options = _environments
                .Environments
                .Select(env => env.Name)
                .ToList();
            var selection = options.IndexOf(_settings.Environment);
            var popupSelection = EditorGUILayout.Popup("Environment", selection, options.ToArray());
            if (selection != popupSelection)
            {
                _settings.Environment = _environments.Environments[popupSelection].Name;
                
                SaveUserSettings(_settings);
            }
        }

        /// <summary>
        /// Draws the connect form.
        /// </summary>
        private void DrawConnectForm()
        {
            EditorGUIUtility.labelWidth = 60;
            
            var environment = _environments.Environment(_settings.Environment);
            if (null == environment)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    
                    GUILayout.Label("Invalid environment selection.");
                    
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                
                return;
            }

            var credentials = _settings.Credentials(_settings.Environment);
            GUILayout.BeginVertical("box");
            {
                credentials.Email = EditorGUILayout.TextField("Email", credentials.Email);
                credentials.Password = EditorGUILayout.TextField("Password", credentials.Password);

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Connect"))
                    {
                        SaveUserSettings(_settings);

                        Connect();
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                DrawMessage();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws message.
        /// </summary>
        private void DrawMessage()
        {
            var credentials = _settings.Credentials(_settings.Environment);
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(_logMessage))
                {
                    GUILayout.Label(_logMessage);
                }
                else if (!string.IsNullOrEmpty(credentials.Token))
                {
                    GUILayout.Label("Sign in successful.");
                }
                else
                {
                    GUILayout.Label("Connect to Trellis.");
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Connects to Trellis. Once connected, automatically saves token.
        /// </summary>
        private void Connect()
        {
            var environment = _environments.Environment(_settings.Environment);
            if (null == environment)
            {
                Log.Warning(this, "Invalid environment selection.");
                return;
            }

            var credentials = _settings.Credentials(_settings.Environment);
            if (null == credentials)
            {
                Log.Warning(this, "Invalid credentials.");
                return;
            }
            
            // setup HTTP
            var builder = EditorApplication.Http.UrlBuilder;
            builder.BaseUrl = "http://" + environment.Hostname;
            builder.Port = environment.Port;
            builder.Version = environment.ApiVersion;
            
            Log.Info(this, "Attempting to connect to Trellis.");
            
            EditorUtility.DisplayProgressBar(
                SIGNIN_PROGRESS_TITLE,
                "Attempting to connect.",
                0.25f);

            // try signing in
            SignIn(credentials)
                .OnSuccess(_ =>
                {
                    EditorUtility.ClearProgressBar();
                    
                    Log.Info(this, "Successfully connected.");
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Signin failed, attempting signup ({0}).", exception.Message);
                    
                    SignUp(credentials)
                        .OnSuccess(__ =>
                        {
                            EditorUtility.ClearProgressBar();
                            
                            Log.Info(this, "Successfully connected to Trellis.");
                        })
                        .OnFailure(error =>
                        {
                            EditorUtility.ClearProgressBar();

                            _logMessage = "Could not signup.";
                            
                            Log.Warning(this, "Could not connect to Trellis.");
                        });
                });
        }

        /// <summary>
        /// Attempts to sign up.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> SignUp(EnvironmentCredentials credentials)
        {
            var token = new AsyncToken<Void>();

            EditorApplication
                .Api
                .EmailAuths
                .EmailSignUp(new Trellis.Messages.EmailSignUp.Request
                {
                    DisplayName = "UnityEditor",
                    Email = credentials.Email,
                    Password = credentials.Password
                })
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess)
                    {
                        if (response.Payload.Success)
                        {
                            credentials.Token = response.Payload.Body.Token;

                            SaveUserSettings(_settings);

                            token.Succeed(Void.Instance);
                        }
                        else
                        {
                            token.Fail(new Exception(response.Payload.Error));
                        }
                    }
                    else
                    {
                        token.Fail(new Exception(response.NetworkError));
                    }
                })
                .OnFailure(token.Fail);
            
            return token;
        }

        /// <summary>
        /// Attempts to sign in.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> SignIn(EnvironmentCredentials credentials)
        {
            var token = new AsyncToken<Void>();
            
            EditorApplication
                .Api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = credentials.Email,
                    Password = credentials.Password
                })
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess)
                    {
                        if (response.Payload.Success)
                        {
                            credentials.Token = response.Payload.Body.Token;

                            SaveUserSettings(_settings);

                            token.Succeed(Void.Instance);
                        }
                        else
                        {
                            token.Fail(new Exception(response.Payload.Error));
                        }
                    }
                    else
                    {
                        token.Fail(new Exception(response.NetworkError));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Loads information about environments.
        /// </summary>
        private void LoadEnvironments()
        {
            var config = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Config/Environments.json");
            if (null == config)
            {
                return;
            }
            
            var bytes = Encoding.UTF8.GetBytes(config.text);
            object @object;
            
            _serializer.Deserialize(
                typeof(EnvironmentConfig),
                ref bytes,
                out @object);

            _environments = (EnvironmentConfig) @object;
            
            Log.Info(this, "Loaded environments : {0}.", _environments);
        }

        /// <summary>
        /// Saves credentials to disk.
        /// </summary>
        /// <param name="credentials">Credentials to save.</param>
        private void SaveUserSettings(UserEnvironmentSettings credentials)
        {
            var path = GetSettingsPath();
            
            Log.Info(this,
                "Saving {0}  to {1}.",
                credentials,
                path);
            
            byte[] bytes;
            _serializer.Serialize(credentials, out bytes);
            var json = Encoding.UTF8.GetString(bytes);

            File.WriteAllText(
                path,
                json);
        }

        /// <summary>
        /// Loads credentials.
        /// </summary>
        private void LoadCredentials()
        {
            // create settings object if one doesn't exist
            var path = GetSettingsPath();
            if (!File.Exists(path))
            {
                SaveUserSettings(new UserEnvironmentSettings());
            }
    
            // load object
            var json = File.ReadAllText(path);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            object @object;
            _serializer.Deserialize(
                typeof(UserEnvironmentSettings),
                ref jsonBytes,
                out @object);
            _settings = (UserEnvironmentSettings) @object;
            
            // make sure there is an entry for every environment
            var changed = false;
            for (int i = 0, len = _environments.Environments.Length; i < len; i++)
            {
                var environment = _environments.Environments[i];
                if (null == _settings.Credentials(environment.Name))
                {
                    _settings.All = _settings.All.Add(new EnvironmentCredentials
                    {
                        Environment = environment.Name
                    });

                    changed = true;
                }
            }

            if (changed)
            {
                SaveUserSettings(_settings);
            }
            
            Log.Info(this, "Loaded credentials {0}.", _settings);
        }

        /// <summary>
        /// Path to credentials.
        /// </summary>
        /// <returns></returns>
        private static string GetSettingsPath()
        {
            var path = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "User.Environment.config");
            return path;
        }
    }
}