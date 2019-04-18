using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Application state that prompts for login with keyboard.
    /// </summary>
    public class KeyboardLoginStrategy : ILoginStrategy
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
        private EditorInputLoginUIView _loginView;

        /// <summary>
        /// Tracks login internally.
        /// </summary>
        private AsyncToken<CredentialsData> _loginToken;
        
        /// <summary>
        /// Id of login view.
        /// </summary>
        private int _loginViewId = -1;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public KeyboardLoginStrategy(
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

            OpenLogin();
            
            return _loginToken.Token();
        }
        
        /// <summary>
        /// Opens login view.
        /// </summary>
        private void OpenLogin()
        {
            if (!_ui.Reveal(_loginViewId))
            {
                _ui
                    .Open<EditorInputLoginUIView>(new UIReference
                    {
                        UIDataId = "Editor.Input"
                    }, out _loginViewId)
                    .OnSuccess(el =>
                    {
                        _loginView = el;
                        _loginView.OnSubmit += Login_OnSubmit;
                    })
                    .OnFailure(ex => Log.Error(this, "Could not open Login.Input : {0}.", ex));   
            }
        }
        
        /// <summary>
        /// Called when the view controller submit button has been pressed.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private void Login_OnSubmit(string username, string password)
        {
            int loadingId;
            _ui
                .Open<IUIElement>(new UIReference
                {
                    UIDataId = UIDataIds.LOADING
                }, out loadingId);
            
            _api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = username,
                    Password = password
                })
                .OnFinally(_ => _ui.Close(loadingId))
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
                        _loginView.errorText = response.Payload.Error;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);
                    _loginView.errorText = "Could not sign in. Please verify your credentials.";
                });
        }
    }
}