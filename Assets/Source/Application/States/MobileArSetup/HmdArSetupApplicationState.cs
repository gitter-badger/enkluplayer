using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using Utils;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Setup for Hmd.
    /// </summary>
    public class HmdArSetupApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IArService _ar;
        private readonly IMessageRouter _messages;
        private readonly ArServiceConfiguration _arConfig;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HmdArSetupApplicationState(
            IArService ar,
            IMessageRouter messages,
            ArServiceConfiguration config)
        {
            _ar = ar;
            _messages = messages;
            _arConfig = config;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            _ar.Setup(_arConfig);
            
            // TODO: find floor
            
            _messages.Publish(MessageTypes.FLOOR_FOUND);
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }
    }
}