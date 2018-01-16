using System;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Window for setting up Trellis integration.
    /// </summary>
    public class TrellisEditorWindow : EditorWindow
    {
        /// <summary>
        /// Tabs.
        /// </summary>
        private readonly TabCollectionComponent _tabs = new TabCollectionComponent();

        /// <summary>
        /// Views to render.
        /// </summary>
        private readonly LoginView _settingsView = new LoginView();
        private readonly HttpControllerView _controllerView = new HttpControllerView();
        private readonly WorldScanView _worldScanView = new WorldScanView();
        
        /// <summary>
        /// Opens window.
        /// </summary>
        [MenuItem("Tools/Trellis Editor %t")]
        private static void Open()
        {
            GetWindow<TrellisEditorWindow>();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("Trellis");
            
            _settingsView.OnRepaintRequested += Repaint;
            _controllerView.OnRepaintRequested += Repaint;
            _tabs.OnRepaintRequested += Repaint;

            _tabs.Tabs = new TabComponent[]
            {
                //new ViewTabComponent("Login", _settingsView),
                new ViewTabComponent("World Scans", _worldScanView),
                //new ViewTabComponent("Controllers", _controllerView)
            };

            _settingsView.OnConnected += Settings_OnConnected;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            EditorUtility.ClearProgressBar();

            _tabs.OnRepaintRequested -= Repaint;
            _controllerView.OnRepaintRequested -= Repaint;
            _settingsView.OnRepaintRequested -= Repaint;
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnGUI()
        {
            GUI.skin = (GUISkin) EditorGUIUtility.Load("Default.guiskin");

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
        private void Settings_OnConnected()
        {
            _worldScanView.Refresh();
        }
    }
}