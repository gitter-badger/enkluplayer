using CreateAR.Commons.Unity.Logging;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Editor Window that sends logs.
    /// </summary>
    public class LoggingEditorWindow : EditorWindow
    {
        /// <summary>
        /// The message to send.
        /// </summary>
        private string _message;

        /// <summary>
        /// The level to send the log at.
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// Menu item that opens window.
        /// </summary>
        [MenuItem("Tools/Log Emulator")]
        private static void Open()
        {
            GetWindow<LoggingEditorWindow>();
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("Log'em");
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnGUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                _message = EditorGUILayout.TextField("Message", _message);
                _level = (LogLevel) EditorGUILayout.EnumPopup("Level", _level);

                if (GUILayout.Button("Send"))
                {
                    Log.Out(_level, this, _message);
                }
            }
            GUILayout.EndVertical();
        }
    }
}
