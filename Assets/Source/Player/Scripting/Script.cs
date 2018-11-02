using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    public abstract class Script : MonoBehaviour
    {
        /// <summary>
        /// Called after script is ready, before FSM flow.
        /// </summary>
        public abstract IAsyncToken<Void> Configure();

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