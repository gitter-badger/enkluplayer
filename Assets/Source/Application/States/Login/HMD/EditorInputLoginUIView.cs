using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls input view.
    /// </summary>
    public class EditorInputLoginUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Email text.
        /// </summary>
        private string _email = "";

        /// <summary>
        /// Password text.
        /// </summary>
        private string _password = "";

        /// <summary>
        /// Error text.
        /// </summary>
        public string errorText = "";

        /// <summary>
        /// Called with username and password.
        /// </summary>
        public event Action<string, string> OnSubmit;

        /// <summary>
        /// Called when submit button has been pressed.
        /// </summary>
        public void Submit_OnClicked()
        {
            if (string.IsNullOrEmpty(_email))
            {
                errorText = "Please enter a valid email.";
                return;
            }

            if (string.IsNullOrEmpty(_password))
            {
                errorText = "Please enter a valid password.";
                return;
            }

            if (null != OnSubmit)
            {
                OnSubmit(_email, _password);
            }
        }

        /// <inheritdoc />
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            GUILayout.Label("Email:");
            _email = GUILayout.TextField(_email, 30, GUILayout.Width(345), GUILayout.Height(30));

            GUILayout.Label("Password:");
            _password = GUILayout.PasswordField(_password, '*', GUILayout.Width(345), GUILayout.Height(30));

            if (GUILayout.Button("Login"))
            {
                errorText = "Signing in...";
                Submit_OnClicked();
            }

            GUILayout.Label(errorText);

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }
    }
}