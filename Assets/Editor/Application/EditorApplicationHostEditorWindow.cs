using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class EditorApplicationHostEditorWindow : EditorWindow
    {
        private bool _isInjected = false;

        private int _sendMessageType;
        private string _sendMessagePayload;
        
        [Inject]
        public IMessageRouter Router { get; set; }
        [Inject]
        public IApplicationState State { get; set; }

        [MenuItem("Window/Application Host Editor")]
        private static void Open()
        {
            GetWindow<EditorApplicationHostEditorWindow>();
        }

        private void OnGUI()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                EditorGUILayout.LabelField("Application is not running.");
                return;
            }

            // fairly evil
            if (!_isInjected)
            {
                _isInjected = true;

                Main.Inject(this);
            }

            EditorGUILayout.BeginHorizontal();
            {
                DrawState();
                DrawSendMessage();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawState()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2f));
            {
                
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSendMessage()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2f));
            {
                _sendMessageType = EditorGUILayout.IntField(
                    "Message Type",
                    _sendMessageType);
                _sendMessagePayload = EditorGUILayout.TextArea(
                    _sendMessagePayload,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(200));

                if (GUILayout.Button("Send"))
                {
                    Router.Publish(
                        _sendMessageType,
                        _sendMessagePayload);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}