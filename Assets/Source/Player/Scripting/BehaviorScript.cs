using CreateAR.EnkluPlayer.IUX;
using Jint;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public abstract class BehaviorScript : MonoBehaviour
    {
        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="jsCache">Js cache.</param>
        /// <param name="factory">Creates elements.</param>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="element">The element.</param>
        public abstract void Initialize(
            IElementJsCache jsCache,
            IElementJsFactory factory,
            Engine engine,
            EnkluScript script,
            Element element);

        /// <summary>
        /// Called after script is ready, before FSM flow.
        /// </summary>
        public abstract void Configure();

        /// <summary>
        /// Enters the script.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void FrameUpdate();

        /// <summary>
        /// Exits the script.
        /// </summary>
        public abstract void Exit();
    }
}