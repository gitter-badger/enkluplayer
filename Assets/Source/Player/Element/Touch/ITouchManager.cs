using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Described an object that can detect touch events on elements.
    /// </summary>
    public interface ITouchManager
    {
        /// <summary>
        /// Registers an element for touch detection.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="delegate">An object that can respond to touch events.</param>
        /// <returns></returns>
        bool Register(Element element, ITouchDelegate @delegate);

        /// <summary>
        /// Unregister an element from touch detection.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="delegate">An object that can respond to touch events.</param>
        /// <returns></returns>
        bool Unregister(Element element);

        /// <summary>
        /// Needs to be called every frame to update touch detection.
        /// </summary>
        void Update();
    }
}