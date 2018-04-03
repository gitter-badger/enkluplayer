using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that orients a user to the HoloLens.
    /// </summary>
    public class OrientationApplicationState : IState
    {
        /// <summary>
        /// Sends/receives messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Root object.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrientationApplicationState(IMessageRouter messages)
        {
            _messages = messages;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Enter {0}.", GetType().Name);

            _root = new GameObject("Orientation");
            _root
                .AddComponent<OrientationViewController>()
                .OnContinue += Orientation_OnContinue;
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            Log.Info(this, "Exit {0}.", GetType().Name);

            Object.Destroy(_root);
        }

        /// <summary>
        /// Called when the orientation view receives a complete.
        /// </summary>
        private void Orientation_OnContinue()
        {
            _messages.Publish(MessageTypes.LOGIN);
        }
    }
}