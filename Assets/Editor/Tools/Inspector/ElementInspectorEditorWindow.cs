using UnityEditor;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Inspector for an element.
    /// </summary>
    public class ElementInspectorEditorWindow : EditorWindow
    {
        /// <summary>
        /// Draws controls for an element.
        /// </summary>
        private readonly ElementView _elementView = new ElementView();
        
        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Tools/Element Inspector")]
        private static void Open()
        {
            GetWindow<ElementInspectorEditorWindow>();
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnEnable()
        {
            _elementView.OnRepaintRequested += Repaint;

            EditorApplication.update += EditorApplication_Update;
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnDisable()
        {
            _elementView.OnRepaintRequested -= Repaint;

            EditorApplication.update -= EditorApplication_Update;
        }

        /// <inheritdoc cref="EditorWindow"/>
        private void OnGUI()
        {
            _elementView.Draw();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void EditorApplication_Update()
        {
            _elementView.Selection = Selection.activeGameObject;
        }
    }
}