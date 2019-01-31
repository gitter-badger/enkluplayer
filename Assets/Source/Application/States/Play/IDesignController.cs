using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    public enum DesignControllerMode
    {
        Normal,
        DebugRendering
    }
    
    /// <summary>
    /// Design mode controller.
    /// </summary>
    public interface IDesignController
    {
        /// <summary>
        /// Starts controllers.
        /// </summary>
        void Setup(DesignerContext context, IAppController app);

        /// <summary>
        /// Tears down controller.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Creates a scene and returns the scene id.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<string> Create();

        /// <summary>
        /// Selects an element.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        /// <param name="elementId">The id of the element.</param>
        void Select(string sceneId, string elementId);

        /// <summary>
        /// Focuses on an element.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        /// <param name="elementId">The id of the element.</param>
        void Focus(string sceneId, string elementId);
    }
}