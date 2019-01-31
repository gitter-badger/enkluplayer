using System;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Window for setting up Enklu integration.
    /// </summary>
    public class EnkluEditorWindow : EditorWindow
    {
        /// <summary>
        /// Tabs.
        /// </summary>
        private readonly TabCollectionComponent _tabs = new TabCollectionComponent();

        /// <summary>
        /// Views to render.
        /// </summary>
        private readonly LoginView _loginView = new LoginView();
        private readonly DetailsView _detailsView = new DetailsView();
        private readonly HttpControllerView _controllerView = new HttpControllerView();
        
        /// <summary>
        /// Opens window.
        /// </summary>
        [MenuItem("Tools/Trellis Editor %t")]
        private static void Open()
        {
            GetWindow<EnkluEditorWindow>();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("Trellis");

            _loginView.OnRepaintRequested += Repaint;
            _detailsView.OnRepaintRequested += Repaint;
            _controllerView.OnRepaintRequested += Repaint;
            _tabs.OnRepaintRequested += Repaint;

            _tabs.Tabs = new TabComponent[]
            {
                new ViewTabComponent("Login", _loginView),
                new ViewTabComponent("Details", _detailsView),
            };

            _loginView.OnConnected += Login_OnConnected;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            EditorUtility.ClearProgressBar();

            _tabs.OnRepaintRequested -= Repaint;
            _controllerView.OnRepaintRequested -= Repaint;
            _detailsView.OnRepaintRequested -= Repaint;
            _loginView.OnRepaintRequested -= Repaint;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnGUI()
        {
            if (!EditorApplication.IsRunning)
            {
                return;
            }
            
            GUI.skin = (GUISkin)EditorGUIUtility.Load("Default.guiskin");

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(10);

                    _tabs.Draw();

                    GUILayout.Space(10);
                }
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Called when we have connected to Trellis.
        /// </summary>
        private void Login_OnConnected()
        {
            Log.Info(this, "Connected to Trellis.");
        }
    }
}