using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Window for setting up Trellis integration.
    /// </summary>
    public class TrellisSettingsWindow : EditorWindow
    {
        /// <summary>
        /// View to render.
        /// </summary>
        private readonly TrellisSettingsView _view = new TrellisSettingsView();

        /// <summary>
        /// Opens window.
        /// </summary>
        [MenuItem("Tools/Settings/Trellis")]
        private static void Open()
        {
            GetWindow<TrellisSettingsWindow>();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            titleContent = new GUIContent("Trellis");

            _view.OnRepaintRequested += Repaint;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            EditorUtility.ClearProgressBar();

            _view.OnRepaintRequested -= Repaint;
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnGUI()
        {
            _view.Draw();
        }
    }
}