namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Stub for mucking with widgets.
    /// </summary>
    [JsInterface("widgets")]
    public class WidgetsScriptingInterface
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IContentManager _content;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">Manages content.</param>
        public WidgetsScriptingInterface(IContentManager content)
        {
            _content = content;
        }

        /// <summary>
        /// Shows a piece of shared content by id.
        /// </summary>
        /// <param name="id">Id of the content to show.</param>
        /// <returns></returns>
        public bool Show(string id)
        {
            var content = _content.FindShared(id);
            if (null != content)
            {
                content.Show();
                return true;
            }

            return false;
        }
    }
}