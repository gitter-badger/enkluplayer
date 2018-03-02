using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a design mode state.
    /// </summary>
    public interface IDesignState : IState
    {
        /// <summary>
        /// Called just before enter.
        /// </summary>
        /// <param name="design">The design controller.</param>
        /// <param name="unityRoot">The root to add unity components.</param>
        /// <param name="dynamicRoot">The root of dynamic menus.</param>
        /// <param name="staticRoot">The root of static menus.</param>
        void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot);
    }
}