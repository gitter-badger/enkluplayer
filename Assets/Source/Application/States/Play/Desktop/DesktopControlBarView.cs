using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Responds to control bar.
    /// </summary>
    public class DesktopControlBarView : MonoBehaviour
    {
        /// <summary>
        /// The system to change.
        /// </summary>
        public EditorGizmoSystem Gizmos;

        /// <summary>
        /// Called when the translate button has been pressed.
        /// </summary>
        public void OnTranslate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Translation);
        }

        /// <summary>
        /// Called when the rotate button has been pressed.
        /// </summary>
        public void OnRotate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Rotation);
        }

        /// <summary>
        /// Called when the scale button has been pressed.
        /// </summary>
        public void OnScale()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Scale);
        }

        /// <summary>
        /// Called when the fullscreen button has been pressed.
        /// </summary>
        public void OnFullScreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}