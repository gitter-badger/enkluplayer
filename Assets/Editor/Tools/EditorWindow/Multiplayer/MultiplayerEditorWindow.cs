using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Tool to visualize info about multiplayer.
    /// </summary>
    public class MultiplayerEditorWindow : EditorWindow
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IMultiplayerController Multiplayer { get; set; }
        [Inject]
        public ApplicationConfig Config { get; set; }

        /// <summary>
        /// Opens the tool.
        /// </summary>
        [MenuItem("Tools/Multiplayer")]
        private static void Open()
        {
            GetWindow<MultiplayerEditorWindow>();
        }

        /// <summary>
        /// Called when enabled.
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Mycelium");
            
            UnityEditor.EditorApplication.update += Repaint;
        }

        /// <summary>
        /// Called when disabled.
        /// </summary>
        private void OnDisable()
        {
            UnityEditor.EditorApplication.update -= Repaint;
        }

        /// <summary>
        /// Draws controls.
        /// </summary>
        private void OnGUI()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            if (null == Multiplayer)
            {
                Main.Inject(this);
            }

            if (null == Multiplayer)
            {
                return;
            }

            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Label("Properties");

                    var env = Config.Network.Environment;

                    GUILayout.Label(env.Name);
                    GUILayout.Label("IP: " + env.MyceliumIp);
                    GUILayout.Label("Port: " + env.MyceliumPort);
                    GUILayout.Label("Connected: " + Multiplayer.IsConnected);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Label("Actions");

                    if (Multiplayer.IsConnected)
                    {
                        if (GUILayout.Button("Disconnect"))
                        {
                            ((MyceliumMultiplayerController) Multiplayer).Tcp.Close();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
    }
}