using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Interface for elements with a GameObject.
    /// </summary>
    public interface IUnityElement
    {
        /// <summary>
        /// The GameObject.
        /// </summary>
        GameObject GameObject { get; }
    }
}