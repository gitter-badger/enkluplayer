using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that just prompts for login.
    /// </summary>
    public class InputLoginStrategy : ILoginStrategy
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Login view.
        /// </summary>
        private InputLoginUIView _view;
        
        /// <summary>
        /// Tracks login internally.
        /// </summary>
        private AsyncToken<CredentialsData> _loginToken;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        public InputLoginStrategy(
            IUIManager ui,
            ApiController api)
        {
            _ui = ui;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            var frame = _ui.CreateFrame();
            
            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnFinally(_ => frame.Release());
            
            _ui
                .Open<InputLoginUIView>(new UIReference
                {
                    UIDataId = "Login.Input"
                })
                .OnSuccess(el =>
                {
                    _view = el;
                    _view.OnSubmit += View_OnSubmit;
                })
                .OnFailure(ex => Log.Error(this, "Could not open Login.Input : {0}.", ex));
            
            return _loginToken.Token();
        }
        
        /// <summary>
        /// Called when the view controller submit button has been pressed.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private void View_OnSubmit(string username, string password)
        {
            _api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = username,
                    Password = password
                })
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        // fill out credentials
                        var creds = new CredentialsData
                        {
                            UserId = response.Payload.Body.User.Id,
                            Token = response.Payload.Body.Token
                        };
                        
                        _loginToken.Succeed(creds);
                    }
                    else
                    {
                        Log.Error(this, "There was an error signing in : {0}.", response.Payload.Error);

                        _view.Error.text = response.Payload.Error;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);

                    _view.Error.text = "Could not sign in. Please try again.";
                });
        }
    }
}