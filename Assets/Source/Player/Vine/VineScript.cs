using CreateAR.EnkluPlayer.IUX;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.Vine;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public abstract class VineScript : MonoBehaviour
    {
        /// <summary>
        /// Initializes script.
        /// </summary>
        public abstract void Initialize(
            Element parent,
            EnkluScript script,
            VineImporter importer,
            IElementFactory elements);

        /// <summary>
        /// Call after script is ready, before FSM flow.
        /// </summary>
        public abstract IAsyncToken<Void> Configure();

        /// <summary>
        /// Enter this script.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void FrameUpdate();

        /// <summary>
        /// Destroys component and created elements.
        /// </summary>
        public abstract void Exit();
    }
}
