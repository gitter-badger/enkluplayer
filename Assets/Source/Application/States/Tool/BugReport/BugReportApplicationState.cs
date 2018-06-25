using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bug report state..
    /// </summary>
    public class BugReportApplicationState : IState
    {
        /// <summary>
        /// Controls UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BugReportApplicationState(
            IUIManager ui,
            IMessageRouter messages)
        {
            _ui = ui;
            _messages = messages;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            _ui
                .Open<SupportInfoUIView>(new UIReference
                {
                    UIDataId = "Tools.BugReport"
                })
                .OnSuccess(el => el.OnClose += () => _messages.Publish(MessageTypes.USER_PROFILE))
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load bug report view : {0}", exception);

                    _messages.Publish(MessageTypes.USER_PROFILE);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }
    }
}