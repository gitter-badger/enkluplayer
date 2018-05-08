using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// MonoBehaviour implementation.
    /// </summary>
    public class MonoBehaviourUIElement : MonoBehaviour, IUIElement
    {
        /// <summary>
        /// Stack id.
        /// </summary>
        public int StackId { get; private set; }

        /// <summary>
        /// Inits the element with a stack id.
        /// </summary>
        /// <param name="stackId">The stack id.</param>
        public void Init(int stackId)
        {
            StackId = stackId;
        }

        /// <inheritdoc />
        public virtual void Created()
        {
            
        }

        /// <inheritdoc />
        public virtual void Added()
        {
            
        }

        /// <inheritdoc />
        public virtual void Revealed()
        {
            
        }

        /// <inheritdoc />
        public virtual void Covered()
        {
            
        }

        /// <inheritdoc />
        public virtual void Removed()
        {
            
        }
    }
}