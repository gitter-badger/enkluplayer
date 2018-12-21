using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    public abstract class Script : MonoBehaviour
    {
        public ScriptData Data { get; protected set; }
        
        public bool IsRunning { get; private set; }
        
        /// <summary>
        /// Called after script is ready, before FSM flow.
        /// </summary>
        public abstract IAsyncToken<Void> Configure();

        /// <summary>
        /// Enters the script.
        /// </summary>
        public virtual void Enter()
        {
            IsRunning = true;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void FrameUpdate();

        /// <summary>
        /// Exits the script.
        /// </summary>
        public virtual void Exit()
        {
            IsRunning = false;
        }
    }
}