using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// State used when there is a version mismatch with the server-- this is
    /// the end of the line!
    /// </summary>
    public class VersionErrorApplicationState : IState
    {
        /// <summary>
        /// Error information.
        /// </summary>
        public class VersionError
        {
            /// <summary>
            /// The message to use.
            /// </summary>
            public string Message;

            /// <summary>
            /// Allows continuing.
            /// </summary>
            public bool AllowContinue;
        }

        /// <summary>
        /// Manipulates UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VersionErrorApplicationState(
            IUIManager ui,
            IMessageRouter messages)
        {
            _ui = ui;
            _messages = messages;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            var error = (VersionError) context;

            _ui
                .Open<ICommonErrorView>(new UIReference
                {
                    UIDataId = UIDataIds.ERROR
                })
                .OnSuccess(el =>
                {
                    el.Message = error.Message;

                    if (error.AllowContinue)
                    {
                        el.Action = "Continue";
                        el.OnOk += () => _messages.Publish(MessageTypes.LOGIN);
                    }
                    else
                    {
                        el.DisableAction();
                    }
                })
                .OnFailure(ex => Log.Fatal(this, "COuld not open error popup : {0}.", ex));
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