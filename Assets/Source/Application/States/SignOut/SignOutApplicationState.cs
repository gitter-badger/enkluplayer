using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// State that signs a user out.
    /// </summary>
    public class SignOutApplicationState : IState
    {
        /// <summary>
        /// Files.
        /// </summary>
        private readonly IFileManager _files;
        
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SignOutApplicationState(
            IFileManager files,
            IMessageRouter messages)
        {
            _files = files;
            _messages = messages;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            // TODO: Are you sure? dialog
            
            // delete saved credentials
            _files
                .Delete(LoginApplicationState.CREDS_URI)
                .OnFailure(exception => Log.Error(this, "Could not delete credentials : {0}.", exception))
                // redirect to login
                .OnFinally(_ => _messages.Publish(MessageTypes.LOGIN));
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