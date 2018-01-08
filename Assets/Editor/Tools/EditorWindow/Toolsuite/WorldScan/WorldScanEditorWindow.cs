using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Hosts the <c>WorldScanView</c>.
    /// </summary>
    public class WorldScanEditorWindow : EditorWindow
    {
        /// <summary>
        /// View to render.
        /// </summary>
        private readonly WorldScanView _view = new WorldScanView();

        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Tools/World Scans")]
        private static void Open()
        {
            GetWindow<WorldScanEditorWindow>();
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("World Scans");

            _view.OnRepaintRequested += Repaint;
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnDisable()
        {
            _view.OnRepaintRequested -= Repaint;
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnGUI()
        {
            _view.Draw();
        }
    }
}