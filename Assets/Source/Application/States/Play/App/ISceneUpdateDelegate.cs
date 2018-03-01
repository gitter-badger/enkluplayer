using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that adds and removes elements from a scene.
    /// </summary>
    public interface ISceneUpdateDelegate
    {
        /// <summary>
        /// Adds an element to a scene.
        /// </summary>
        /// <param name="sceneId">The scene to add to.</param>
        /// <param name="data">The data.</param>
        IAsyncToken<Element> Add(string sceneId, ElementData data);

        /// <summary>
        /// Removes an element from a scene.
        /// </summary>
        /// <param name="sceneId">The scene to remove from.</param>
        /// <param name="element">The element.</param>
        IAsyncToken<Element> Remove(string sceneId, Element element);
    }
}