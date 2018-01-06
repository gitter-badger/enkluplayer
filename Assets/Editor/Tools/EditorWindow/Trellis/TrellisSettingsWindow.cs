using System;
using System.IO;
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
        /// All environments.
        /// </summary>
        private EnvironmentConfig _environments;
        
        /// <summary>
        /// Settings!
        /// </summary>
        private TrellisCredentials _credentials;

        /// <summary>
        /// Message to display.
        /// </summary>
        private string _errorMessage;
        
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
            EditorGUIUtility.labelWidth = 60;
            
            GUILayout.BeginVertical();
            {
                DrawConnectForm();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the connect form.
        /// </summary>
        private void DrawConnectForm()
        {
            GUILayout.BeginVertical("box");
            {
                _credentials.Email = EditorGUILayout.TextField("Email", _credentials.Email);
                _credentials.Password = EditorGUILayout.TextField("Password", _credentials.Password);

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Connect"))
                    {
                        SaveCredentials(_credentials);

                        GetToken();
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
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    GUILayout.Label(_errorMessage);
                }
                else if (!string.IsNullOrEmpty(_credentials.Token))
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

        private void GetToken()
        {
            EditorUtility.DisplayProgressBar(
                SIGNIN_PROGRESS_TITLE,
                "Attempting to sign in.",
                0.25f);

            // try signing in
            SignIn()
                .OnSuccess(_ => EditorUtility.ClearProgressBar())
                .OnFailure(_ =>
                {
                    SignUp()
                        .OnSuccess(__ =>
                        {
                            EditorUtility.ClearProgressBar();
                        })
                        .OnFailure(error =>
                        {
                            EditorUtility.ClearProgressBar();

                            _errorMessage = "Could not signup.";
                        });
                });
        }

        /// <summary>
        /// Attempts to sign up.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> SignUp()
        {
            var token = new AsyncToken<Void>();

            EditorApplication
                .Api
                .EmailAuths
                .EmailSignUp(new Trellis.Messages.EmailSignUp.Request
                {
                    DisplayName = "UnityEditor",
                    Email = _credentials.Email,
                    Password = _credentials.Password
                })
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess && response.Payload.Success)
                    {
                        _credentials.Token = response.Payload.Body.Token;

                        SaveCredentials(_credentials);
                        
                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.NetworkError ?? response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);
            
            return token;
        }

        /// <summary>
        /// Attempts to sign in.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> SignIn()
        {
            var token = new AsyncToken<Void>();
            
            EditorApplication
                .Api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = _credentials.Email,
                    Password = _credentials.Password
                })
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess && response.Payload.Success)
                    {
                        _credentials.Token = response.Payload.Body.Token;

                        SaveCredentials(_credentials);

                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.NetworkError ?? response.Payload.Error));
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
            
        }

        /// <summary>
        /// Saves credentials to disk.
        /// </summary>
        /// <param name="credentials">Credentials to save.</param>
        private void SaveCredentials(TrellisCredentials credentials)
        {
            var path = GetCredentialsPath();
            
            byte[] bytes;
            _serializer.Serialize(credentials, out bytes);
            var json = Encoding.UTF8.GetString(bytes);

            File.WriteAllText(
                path,
                json);
            
            Log.Info(this,
                "Saved {0} to {1}. Output json = '{2}'.",
                credentials,
                path,
                json);
        }

        /// <summary>
        /// Loads credentials.
        /// </summary>
        private void LoadCredentials()
        {
            // load/create settings object
            var path = GetCredentialsPath();
            if (!File.Exists(path))
            {
                SaveCredentials(new TrellisCredentials());
            }

            var json = File.ReadAllText(path);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            object @object;
            _serializer.Deserialize(
                typeof(TrellisCredentials),
                ref jsonBytes,
                out @object);

            _credentials = (TrellisCredentials) @object;
            
            Log.Info(this, "Loaded credentials {0}.", _credentials);
        }

        /// <summary>
        /// Path to credentials.
        /// </summary>
        /// <returns></returns>
        private static string GetCredentialsPath()
        {
            var path = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Trellis.config");
            return path;
        }
    }
}