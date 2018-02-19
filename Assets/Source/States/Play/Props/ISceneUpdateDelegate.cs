using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that listens for adding + removing <c>ElementData</c>
    /// to and from scenes.
    /// </summary>
    public interface ISceneUpdateDelegate
    {
        /// <summary>
        /// Called when <c>ElementData</c> has been added.
        /// </summary>
        /// <param name="scene">The scene to add to.</param>
        /// <param name="data">The data.</param>
        IAsyncToken<Void> Add(SceneController scene, ElementData data);

        /// <summary>
        /// Called when an <c>Element</c> has been removed.
        /// </summary>
        /// <param name="scene">The scene to remove from.</param>
        /// <param name="element">The element.</param>
        IAsyncToken<Void> Remove(SceneController scene, Element element);
    }
}