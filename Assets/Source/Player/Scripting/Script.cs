using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// A base class for all instances of EnkluScripts.
    /// </summary>
    public abstract class Script
    {
        /// <summary>
        /// Retrieves the <c>EnkluScript</c> instance.
        /// </summary>
        public EnkluScript EnkluScript { get; protected set; }
        
        /// <summary>
        /// Whether or not this instance has finished configuration.
        /// </summary>
        public bool IsConfigured { get; protected set; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("<Script Name={0} Version={1} Id= {2} />", EnkluScript.Data.Name,
                EnkluScript.Data.Version, EnkluScript.Data.Id);
        }
    }
}