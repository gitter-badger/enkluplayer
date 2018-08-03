using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls input view.
    /// </summary>
    public class EditorInputLoginUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// The username field.
        /// </summary>
        public InputField UserName;
        
        /// <summary>
        /// The password field.
        /// </summary>
        public InputField Password;

        /// <summary>
        /// Error field.
        /// </summary>
        public Text Error;

        /// <summary>
        /// Called with username and password.
        /// </summary>
        public event Action<string, string> OnSubmit;

        /// <summary>
        /// Called to go to signup.
        /// </summary>
        public event Action OnSignUp;

        /// <summary>
        /// Called to continue as a guest.
        /// </summary>
        public event Action OnContinueAsGuest;

        private string formNick = "";
        private string formPassword = "";

        /// <summary>
        /// Called when submit button has been pressed.
        /// </summary>
        public void Submit_OnClicked()
        {
            if (string.IsNullOrEmpty(UserName.text))
            {
                Error.text = "Please enter a valid email.";
                return;
            }

            if (string.IsNullOrEmpty(Password.text))
            {
                Error.text = "Please enter a valid password.";
                return;
            }
            
            if (null != OnSubmit)
            {
                OnSubmit(UserName.text, Password.text); 
            }
        }
        public void OnGUI()
        {
          //  GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Username:"); //text with your nick
            formNick = GUILayout.TextField(formNick, 30, GUILayout.Width(345), GUILayout.Height(35));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Password:");
            formPassword = GUILayout.PasswordField(formPassword, '*', GUILayout.Width(345), GUILayout.Height(35)); //same as above, but for password
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Login"))//just a button
            {
                OnSubmit(formNick, formPassword);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
           // GUILayout.EndArea();
        }

        private void Login()
        {
            Debug.Log("Logged in."+formNick+" "+formPassword);
        }

        /// <summary>
        /// Called when the signup button has been pressed.
        /// </summary>
        public void OnSignupClicked()
        {
            if (null != OnSignUp)
            {
                OnSignUp();
            }
        }

        /// <summary>
        /// Called when continue has been clicked.
        /// </summary>
        public void ContinueClicked()
        {
            if (null != OnContinueAsGuest)
            {
                OnContinueAsGuest();
            }
        }

        /// <inheritdoc />
        public override void Removed()
        {
            base.Removed();
        
            Error.text = string.Empty;
        }
    }
}