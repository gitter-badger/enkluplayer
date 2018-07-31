using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates a new anchor.
    /// </summary>
    public class CreateNewAppDesignState : IArDesignState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Token to get my apps.
        /// </summary>
        private IAsyncToken<Response> _myAppsToken;

        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CreateNewAppDesignState(ApiController api,
            IUIManager ui)
        {
            _ui = ui;
            _api = api;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entered {0}.", GetType().Name);
            _frame = _ui.CreateFrame();
            OpenAppCreationDialog();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // 
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }

        /// <inheritdoc />
        public void Initialize(HmdDesignController designer, GameObject unityRoot, Element dynamicRoot, Element staticRoot)
        {
            _design = designer;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            // 
        }

        /// <summary>
        /// Opens the UI for create
        /// </summary>
        private void OpenAppCreationDialog()
        {
            //TODO: Voice input widges to get app name and description

            int createAppUiId;
            _ui
                .Open<HmdAppCreationUIView>(new UIReference
                {
                    UIDataId = "Design.CreateApp"
                }, out createAppUiId)
                .OnSuccess(el =>
                {
                    el.OnOk += CreateNewApp;
                    el.OnCancel += BackTo_MainMenu;
                })
                .OnFailure(ex => Log.Error(this, "Could not open Create App UI : {0}.", ex));
        }

        /// <summary>
        /// Creates new app with params
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appDescription"></param>
        private void CreateNewApp(string appName, string appDescription)
        {
            Trellis.Messages.CreateApp.Request request = new Trellis.Messages.CreateApp.Request();
            request.Description = appDescription;
            request.Name = appName;

            _api.Apps.CreateApp(request).OnSuccess(response =>
            {
                if (response.StatusCode == 200)
                {
                    Log.Info(this, "Create new App: {0} successfully", appName);
                    _design.ChangeState<AppListViewDesignState>();
                }
                else
                {
                    Log.Info(this, "Could not create new App : {0}", response);
                    BackTo_MainMenu();
                }

            }).OnFailure(ex =>
            {
                Log.Error(this, "Could not create new App : {0}.", ex);
                int errorId;
                _ui
                    .Open<ICommonErrorView>(
                        new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        },
                        out errorId)
                    .OnSuccess(popup =>
                    {
                        popup.Message = "Could not create the app. Are you sure you're online?";
                        popup.Action = "Retry";
                        popup.OnOk += () =>
                        {
                            _ui.Close(errorId);
                            CreateNewApp(appName, appDescription);
                        };
                    })
                    .OnFailure(er =>
                    {
                        Log.Fatal(this, "Could not open error popup : {0}.", er);
                    });
            });
        }

        /// <summary>
        /// Called when the user selects cancel.
        /// </summary>
        private void BackTo_MainMenu()
        {
            _design.ChangeState<MainDesignState>();
        }
    }
}