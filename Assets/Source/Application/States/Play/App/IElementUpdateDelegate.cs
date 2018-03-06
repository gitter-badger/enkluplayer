using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for an object that handles <c>ElementData</c> updates.
    /// </summary>
    public interface IElementUpdateDelegate
    {
        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, string value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, int value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, float value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, bool value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, Vec3 value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, Col4 value);

        /// <summary>
        /// Finalizes update.
        /// </summary>
        void Finalize(Element element);
    }
}