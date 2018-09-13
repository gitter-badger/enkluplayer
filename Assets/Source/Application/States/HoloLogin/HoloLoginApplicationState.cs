using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.HoloAuthorize;

namespace CreateAR.EnkluPlayer.States.HoloLogin
{
    /// <summary>
    /// Hololens login.
    /// </summary>
    public class HoloLoginApplicationState : IState
    {
        /// <summary>
        /// UI entry point.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Pub/sub.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;
        
        /// <summary>
        /// Frame object.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLoginApplicationState(
            IUIManager ui,
            IMessageRouter messages,
            ApiController api)
        {
            _ui = ui;
            _messages = messages;
            _api = api;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            _ui.Open<ICommonLoadingView>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            });
            
            // first, retrieve a holologin code
            _api
                .HoloAuths
                .HoloAuthorize(new Request())
                .OnSuccess(response =>
                {
                    _ui
                        .Open<MobileHoloLoginUIView>(new UIReference
                        {
                            UIDataId = "HoloLogin"
                        })
                        .OnSuccess(el =>
                        {
                            el.Code = response.Payload.Body;
                            el.OnOk += () => _messages.Publish(MessageTypes.USER_PROFILE);
                        })
                        .OnFailure(exception => Log.Error(this, "Could not open MobileHoloLoginUIView : {0}.", exception));
                })
                .OnFailure(exception =>
                {
                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        })
                        .OnSuccess(err =>
                        {
                            err.Message = "Could not retrieve holocode. Are you sure you're online?";
                            err.OnOk += () => _messages.Publish(MessageTypes.USER_PROFILE);
                        });
                            
                    Log.Error(this, "Could not open HoloLogin : {0}.", exception);
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