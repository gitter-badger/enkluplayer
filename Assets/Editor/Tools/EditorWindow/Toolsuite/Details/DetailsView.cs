using System;
using CreateAR.Commons.Unity.Editor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Displays basic details about Trellis connection.
    /// </summary>
    public class DetailsView : IEditorView
    {
        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;

        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            GUILayout.BeginVertical();
            {
                var env = EditorApplication.Config.Environment;
                if (null == env)
                {
                    GUILayout.Label("Invalid AppConfig: no environment to connect to.");
                }
                else
                {
                    GUILayout.Label(string.Format(
                        "Connected to '{0}'.",
                        env.Name));
                    GUILayout.Label(string.Format("Url: {0}", env.Url));
                }

                var creds = EditorApplication.Config.Credentials;
                if (null == creds)
                {
                    GUILayout.Label("Invalid credentials.");
                }
                else
                {
                    GUILayout.Label(string.Format(
                        "UserId: {0}",
                        creds.UserId));
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Safely called repaint event.
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
