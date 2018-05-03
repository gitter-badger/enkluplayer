using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates scenes from data and manages them.
    /// </summary>
    public class AppSceneManager : IAppSceneManager
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Lookup from sceneId -> root element.
        /// </summary>
        private readonly Dictionary<string, Element> _scenes = new Dictionary<string, Element>();

        /// <inheritdoc />
        public string[] All
        {
            get
            {
                return _scenes.Keys.ToArray();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppSceneManager(IElementFactory elements)
        {
            _elements = elements;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId, IAppDataLoader appData)
        {
            var token = new AsyncToken<Void>();

            foreach (var sceneId in appData.Scenes)
            {
                var description = appData.Scene(sceneId);

                _scenes[sceneId] = _elements.Element(description);
            }

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Uninitialize()
        {
            var token = new AsyncToken<Void>();

            // unload all scenes
            foreach (var scene in _scenes.Values)
            {
                scene.Destroy();
            }
            _scenes.Clear();

            return token;
        }

        /// <inheritdoc />
        public Element Root(string sceneId)
        {
            Element element;
            if (_scenes.TryGetValue(sceneId, out element))
            {
                return element;
            }

            return null;
        }
    }
}