using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Mobile;
using CreateAR.Trellis.Messages;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state for guests to view public apps.
    /// </summary>
    public class GuestApplicationState : IState
    {
        /// <summary>
        /// UI entrypoint.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// UI Frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Search view.
        /// </summary>
        private MobileAppSearchUIView _view;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GuestApplicationState(
            IUIManager ui,
            ApiController api)
        {
            _ui = ui;
            _api = api;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            // generate a frame
            _frame = _ui.CreateFrame();
            
            // TODO: retrieve public apps

            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            });
            
            // open UI
            _ui
                .Open<MobileAppSearchUIView>(new UIReference
                {
                    UIDataId = "App.Search"
                })
                .OnSuccess(el =>
                {
                    _view = el;
                    _view.OnAppSelected += View_OnAppSelected;
                    _view.OnQueryUpdated += View_OnQueryUpdated;
                })
                .OnFailure(exception => Log.Error(this, "Could not open search view : {0}.", exception));
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
        
        /// <summary>
        /// Called when the search query has changed.
        /// </summary>
        /// <param name="query">The new query.</param>
        private void View_OnQueryUpdated(string query)
        {
            
        }

        /// <summary>
        /// Called when an app has been selected.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        private void View_OnAppSelected(string appId)
        {
            
        }
    }
}