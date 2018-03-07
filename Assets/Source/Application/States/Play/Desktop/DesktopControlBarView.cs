using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class DesktopControlBarView : MonoBehaviour
    {
        public EditorGizmoSystem Gizmos;

        public void OnTranslate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Translation);
        }

        public void OnRotate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Rotation);
        }

        public void OnScale()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Scale);
        }

        public void OnFullScreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}