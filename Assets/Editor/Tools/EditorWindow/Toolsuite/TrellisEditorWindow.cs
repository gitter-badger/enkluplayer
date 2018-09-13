using System;
using System.Collections;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Window for setting up Trellis integration.
    /// </summary>
    public class TrellisEditorWindow : EditorWindow
    {
        /// <summary>
        /// How often, in seconds, to poll for new worldscans.
        /// </summary>
        private const int WORLDSCAN_POLL_SEC = 2;
        private const int WORLDSCAN_POLL_TIMEOUT_SEC = 5;

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
        private readonly WorldScanView _worldScanView = new WorldScanView();

        /// <summary>
        /// Polls world scans.
        /// </summary>
        private bool _pollWorldScans;

        /// <summary>
        /// Last time OnGUI was called.
        /// </summary>
        private DateTime _lastDraw;

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

            _loginView.OnRepaintRequested += Repaint;
            _detailsView.OnRepaintRequested += Repaint;
            _controllerView.OnRepaintRequested += Repaint;
            _tabs.OnRepaintRequested += Repaint;

            _tabs.Tabs = new TabComponent[]
            {
                //new ViewTabComponent("Login", _loginView),
                new ViewTabComponent("Details", _detailsView),
                new ViewTabComponent("World Scans", _worldScanView),
                //new ViewTabComponent("Controllers", _controllerView)
            };

            _loginView.OnConnected += Login_OnConnected;

            // start timer for worldscan refresh
            EditorApplication.Bootstrapper.BootstrapCoroutine(UpdateWorldScans());
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            EditorUtility.ClearProgressBar();

            _tabs.OnRepaintRequested -= Repaint;
            _controllerView.OnRepaintRequested -= Repaint;
            _detailsView.OnRepaintRequested -= Repaint;
            _loginView.OnRepaintRequested -= Repaint;

            _pollWorldScans = false;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnGUI()
        {
            if (!EditorApplication.IsRunning)
            {
                return;
            }

            _lastDraw = DateTime.Now;

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
            //
        }

        /// <summary>
        /// Automatically updates worldscans.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateWorldScans()
        {
            _pollWorldScans = true;

            var lastUpdate = DateTime.MinValue;
            while (_pollWorldScans)
            {
                var now = DateTime.Now;
                if (now.Subtract(lastUpdate).TotalSeconds > WORLDSCAN_POLL_SEC)
                {
                    if (now.Subtract(_lastDraw).TotalSeconds < WORLDSCAN_POLL_TIMEOUT_SEC)
                    {
                        lastUpdate = now;

                        _worldScanView.Refresh();
                    }
                }

                yield return null;
            }
        }
    }
}