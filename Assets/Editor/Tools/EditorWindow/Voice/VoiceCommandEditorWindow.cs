using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Allows calling voice commands in the editor.
    /// </summary>
    public class VoiceCommandEditorWindow : EditorWindow
    {
        /// <summary>
        /// Toggles the voice command window.
        /// </summary>
        [MenuItem("Tools/Voice Debugger")]
        private static void Toggle()
        {
            GetWindow<VoiceCommandEditorWindow>();
        }

        /// <summary>
        /// Called when the window is first created.
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Voice");
        }

        /// <summary>
        /// Called to draw controls.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                if (!UnityEngine.Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Application must be in play mode.");
                }
                else
                {
                    DrawCommands();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws buttons for all the commands.
        /// </summary>
        private void DrawCommands()
        {
            var manager = EditorVoiceCommandManager.Instance;
            if (null == manager)
            {
                return;
            }

            var keys = manager.Callbacks.Keys.ToArray();
            foreach (var registration in keys)
            {
                EditorGUILayout.BeginHorizontal("box");
                {
                    EditorGUILayout.LabelField(registration);

                    if (GUILayout.Button("Trigger"))
                    {
                        manager.Call(registration);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            keys = manager.AdminCallbacks.Keys.ToArray();
            foreach (var registration in keys)
            {
                EditorGUILayout.BeginHorizontal("box");
                {
                    EditorGUILayout.LabelField("(Admin) " + registration);

                    if (GUILayout.Button("Trigger"))
                    {
                        manager.Call(registration);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}