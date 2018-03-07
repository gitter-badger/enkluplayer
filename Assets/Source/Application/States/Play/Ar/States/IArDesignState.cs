using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a design mode state.
    /// </summary>
    public interface IArDesignState : IState
    {
        /// <summary>
        /// Called once for the lifetime of the object to initialize the state.
        /// </summary>
        /// <param name="designer">Design controller.</param>
        /// <param name="unityRoot">The root to add unity components.</param>
        /// <param name="dynamicRoot">The root of dynamic menus.</param>
        /// <param name="staticRoot">The root of static menus.</param>
        void Initialize(
            ArDesignController designer,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot);
    }
}