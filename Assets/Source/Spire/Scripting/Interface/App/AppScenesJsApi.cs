namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Js API for scenes.
    /// </summary>
    public class AppScenesJsApi
    {
        /// <summary>
        /// Cache of ElementJs objects.
        /// </summary>
        private readonly IElementJsCache _cache;

        /// <summary>
        /// Scene API.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Retrieves id of all scenes.
        /// </summary>
        public string[] all
        {
            get { return _scenes.All; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppScenesJsApi(
            IElementJsCache cache,
            IAppSceneManager scenes)
        {
            _cache = cache;
            _scenes = scenes;
        }

        /// <summary>
        /// Root element of scene.
        /// </summary>
        /// <param name="sceneId">Id of the scene.</param>
        /// <returns></returns>
        public ElementJs root(string sceneId)
        {
            return _cache.Element(_scenes.Root(sceneId));
        }
    }
}