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
        /// Fullscreen button.
        /// </summary>
        public OnPointerDownButton FullScreen;
        
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
        
        /// <inheritdoc cref="MonoBehaviour" />
        private void Awake()
        {
            FullScreen.OnClicked += () =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                Cursor.lockState = Cursor.lockState == CursorLockMode.Confined
                    ? CursorLockMode.None
                    : CursorLockMode.Confined;
            };
        }
    }
}