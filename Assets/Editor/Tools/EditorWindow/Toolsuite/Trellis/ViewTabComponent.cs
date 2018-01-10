using CreateAR.Commons.Unity.Editor;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// <c>TabComponent</c> that wraps an <c>IEditorView</c>.
    /// </summary>
    public class ViewTabComponent : TabComponent
    {
        /// <summary>
        /// View implementation.
        /// </summary>
        private readonly IEditorView _view;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewTabComponent(
            string label,
            IEditorView view)
        {
            _view = view;

            Label = label;

            _view.OnRepaintRequested += Repaint;
        }

        /// <inheritdoc cref="IEditorView"/>
        public override void Draw()
        {
            base.Draw();

            _view.Draw();
        }
    }
}