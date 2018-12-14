using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that can respond to touches.
    /// </summary>
    public interface ITouchDelegate
    {
        /// <summary>
        /// Called when an element has been touched.
        /// </summary>
        /// <param name="element">The element that was touched.</param>
        /// <param name="intersection">The intersection.</param>
        void TouchStarted(Element element, Vector3 intersection);

        /// <summary>
        /// Called when an element has stopped being touched.
        /// </summary>
        /// <param name="element">The element that was touched.</param>
        void TouchStopped(Element element);

        /// <summary>
        /// Called between started and stopped.
        /// </summary>
        /// <param name="element">The element that was touched.</param>
        /// <param name="intersection">The intersection.</param>
        void TouchDragged(Element element, Vector3 intersection);
    }
}