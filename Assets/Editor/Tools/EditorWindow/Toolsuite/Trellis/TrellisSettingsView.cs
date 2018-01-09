using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.EmailSignIn;
using UnityEditor;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Draws controls for setting up Trellis integration.
    /// </summary>
    public class TrellisSettingsView : IEditorView
    {
        /// <summary>
        /// Message to display.
        /// </summary>
        private string _logMessage;

        /// <summary>
        /// Position.
        /// </summary>
        private Vector2 _scrollPosition;

        /// <summary>
        /// Title for signin.
        /// </summary>
        private const string SIGNIN_PROGRESS_TITLE = "Connecting to Trellis...";

        /// <summary>
        /// Called when connected.
        /// </summary>
        public event Action OnConnected;

        ///<inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;
        
        ///<inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                DrawEnvironmentSelection();
                DrawConnectForm();
                DrawUserInformation();
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Connects to Trellis. Once connected, automatically saves token.
        /// </summary>
        public void Connect()
        {
            var environment = EditorApplication.Environments.Environment(
                EditorApplication.UserSettings.Environment);
            if (null == environment)
            {
                Log.Warning(this, "Invalid environment selection.");
                return;
            }

            var credentials = EditorApplication.UserSettings.Credentials(
                EditorApplication.UserSettings.Environment);
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

                    _logMessage = "Successfully connected.";
                    Log.Info(this, "Successfully connected.");

                    if (null != OnConnected)
                    {
                        OnConnected();
                    }

                    Repaint();
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Signin failed, attempting signup ({0}).", exception.Message);

                    SignUp(credentials)
                        .OnSuccess(__ =>
                        {
                            EditorUtility.ClearProgressBar();

                            Log.Info(this, "Successfully connected to Trellis.");
                            _logMessage = "Successfully connected.";

                            if (null != OnConnected)
                            {
                                OnConnected();
                            }

                            Repaint();
                        })
                        .OnFailure(error =>
                        {
                            EditorUtility.ClearProgressBar();

                            _logMessage = "Could not signup.";

                            Log.Warning(this, "Could not connect to Trellis.");

                            Repaint();
                        });
                });
        }

        /// <summary>
        /// Draws environment selection panel.
        /// </summary>
        private void DrawEnvironmentSelection()
        {
            EditorGUIUtility.labelWidth = 100;

            var options = EditorApplication
                .Environments
                .Environments
                .Select(env => env.Name)
                .ToList();
            var selection = options.IndexOf(EditorApplication.UserSettings.Environment);
            var popupSelection = EditorGUILayout.Popup("Environment", selection, options.ToArray());
            if (selection != popupSelection)
            {
                EditorApplication.UserSettings.Environment = EditorApplication.Environments.Environments[popupSelection].Name;
                EditorApplication.SaveUserSettings();
            }
        }

        /// <summary>
        /// Draws the connect form.
        /// </summary>
        private void DrawConnectForm()
        {
            EditorGUIUtility.labelWidth = 60;

            var environment = EditorApplication.Environments.Environment(
                EditorApplication.UserSettings.Environment);
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

            var credentials = EditorApplication.UserSettings.Credentials(
                EditorApplication.UserSettings.Environment);
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
                        EditorApplication.SaveUserSettings();

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
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(_logMessage))
                {
                    GUILayout.Label(_logMessage);
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
        /// Draws information about user.
        /// </summary>
        private void DrawUserInformation()
        {
            var credentials = EditorApplication.UserSettings.Credentials(
                EditorApplication.UserSettings.Environment);

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("User Id:");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(credentials.UserId);

                    if (GUILayout.Button("Copy"))
                    {
                        EditorGUIUtility.systemCopyBuffer = credentials.UserId;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Token:");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (string.IsNullOrEmpty(credentials.Token))
                    {
                        GUILayout.Label("None");
                    }
                    else
                    {
                        GUILayout.Label(credentials.Token.Substring(0, 24) + "...");
                    }

                    if (GUILayout.Button("Copy"))
                    {
                        EditorGUIUtility.systemCopyBuffer = credentials.Token;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
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
                            credentials.UserId = response.Payload.Body.User.Id;

                            EditorApplication.SaveUserSettings();

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
                            credentials.UserId = response.Payload.Body.User.Id;

                            EditorApplication.SaveUserSettings();

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
        /// Calls repaint event safely.
        /// </summary>
        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }
    }
}