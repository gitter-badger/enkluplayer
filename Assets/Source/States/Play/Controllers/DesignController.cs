using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design mode menus.
    /// </summary>
    public class DesignController
    {
        /// <summary>
        /// Elements!
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Manages props.
        /// </summary>
        private readonly IPropManager _propManager;
        
        /// <summary>
        /// Play mode.
        /// </summary>
        private PlayModeConfig _playConfig;

        /// <summary>
        /// Event handler.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// Splash menu.
        /// </summary>
        private SplashMenuController _splash;

        /// <summary>
        /// Main menu.
        /// </summary>
        private MainMenuController _mainMenu;

        /// <summary>
        /// Clear all menu.
        /// </summary>
        private ClearAllPropsController _clearAllProps;

        /// <summary>
        /// Quit menu.
        /// </summary>
        private QuitController _quit;

        /// <summary>
        /// New item menu.
        /// </summary>
        private NewItemController _new;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private PlaceObjectController _place;

        /// <summary>
        /// Menu to adjust prop.
        /// </summary>
        private PropAdjustController _propAdjust;

        /// <summary>
        /// Menu to edit prop.
        /// </summary>
        private PropEditController _propEdit;

        /// <summary>
        /// Container for all.
        /// </summary>
        private FloatWidget _float;

        /// <summary>
        /// Constuctor.
        /// </summary>
        public DesignController(
            IElementFactory elements,
            IPropManager propManager)
        {
            _elements = elements;
            _propManager = propManager;
        }

        /// <summary>
        /// Starts controllers.
        /// </summary>
        public void Setup(PlayModeConfig config)
        {
            _playConfig = config;
            _events = _playConfig.Events;
            
            // create float
            _float = (FloatWidget) _elements.Element(@"<Float id='Root' position=(0, 0, 1) />");
            _float.GameObject.transform.parent = _events.transform;

            SetupMenus();

            // initialize with hardcoded app id
            _propManager
                .Initialize("test")
                .OnSuccess(_ =>
                {
                    Log.Info(this, "IPropManager initialized.");

                    // create a default propset if there isn't one
                    if (null == _propManager.Active)
                    {
                        Log.Info(this, "No active PropSet, creating a default.");

                        _propManager
                            .Create()
                            .OnSuccess(set => Start())
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not create PropSet!");

                                _splash.Show();
                            });
                    }
                    else
                    {
                        Start();
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, string.Format(
                        "Could not initialize IPropManager : {0}.",
                        exception));
                });
        }
        
        /// <summary>
        /// Tears down all controllers and destroys them.
        /// </summary>
        public void Teardown()
        {
            _splash.Uninitialize();
            _mainMenu.Uninitialize();
            _clearAllProps.Uninitialize();
            _quit.Uninitialize();
            _new.Uninitialize();

            Object.Destroy(_splash.gameObject);
            Object.Destroy(_mainMenu.gameObject);
            Object.Destroy(_clearAllProps.gameObject);
            Object.Destroy(_quit.gameObject);
            Object.Destroy(_new.gameObject);
        }

        /// <summary>
        /// Creates and initializes all menus.
        /// </summary>
        private void SetupMenus()
        {
            var parent = _float;

            _splash = Object.Instantiate(_playConfig.SplashMenu, _events.transform);
            _splash.OnOpenMenu += Splash_OnOpenMenu;
            _splash.Initialize(_events, parent);

            _mainMenu = Object.Instantiate(_playConfig.MainMenu, _events.transform);
            _mainMenu.OnBack += MainMenu_OnBack;
            _mainMenu.OnQuit += MainMenu_OnQuit;
            _mainMenu.OnClearAll += MainMenu_OnClearAll;
            _mainMenu.OnNew += MainMenu_OnNew;
            _mainMenu.Initialize(_events, parent);

            _clearAllProps = Object.Instantiate(_playConfig.ClearAllMenu, _events.transform);
            _clearAllProps.OnCancel += ClearAll_OnCancel;
            _clearAllProps.OnConfirm += ClearAll_OnConfirm;
            _clearAllProps.Initialize(_events, parent);

            _quit = Object.Instantiate(_playConfig.QuitMenu, _events.transform);
            _quit.OnCancel += Quit_OnCancel;
            _quit.OnConfirm += Quit_OnConfirm;
            _quit.Initialize(_events, parent);

            _new = Object.Instantiate(_playConfig.NewMenu, _events.transform);
            _new.OnCancel += New_OnCancel;
            _new.OnConfirm += New_OnConfirm;
            _new.Initialize(_events, parent);
        }

        /// <summary>
        /// Closes prop menus.
        /// </summary>
        private void ClosePropControls()
        {
            if (null != _propAdjust)
            {
                Object.Destroy(_propAdjust.gameObject);
                _propAdjust = null;
            }
        }

        /// <summary>
        /// Listens to prop.
        /// </summary>
        /// <param name="controller">The PropController.</param>
        private void ListenToProp(PropController controller)
        {
            controller.OnAdjust += Controller_OnAdjust;
        }

        /// <summary>
        /// Starts design mode after everything is ready.
        /// </summary>
        private void Start()
        {
            // listen to all prop controllers
            var controllers = _propManager.Active.Props;
            for (int i = 0, len = controllers.Count; i < len; i++)
            {
                ListenToProp(controllers[i]);
            }

            _splash.Show();
        }

        /// <summary>
        /// Called when splash wants to open the menu.
        /// </summary>
        private void Splash_OnOpenMenu()
        {
            _splash.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when main menu wants to go back.
        /// </summary>
        private void MainMenu_OnBack()
        {
            _mainMenu.Hide();
            _splash.Show();
        }

        /// <summary>
        /// Called when main menu wants to quit.
        /// </summary>
        private void MainMenu_OnQuit()
        {
            _mainMenu.Hide();
            _quit.Show();
        }

        /// <summary>
        /// Called when main menu wants to clear all props.
        /// </summary>
        private void MainMenu_OnClearAll()
        {
            _mainMenu.Hide();
            _clearAllProps.Show();
        }

        /// <summary>
        /// Called when main menu wants to create a new prop.
        /// </summary>
        private void MainMenu_OnNew()
        {
            _mainMenu.Hide();
            _new.Show();
        }

        /// <summary>
        /// Called when clear all menu wants to cancel.
        /// </summary>
        private void ClearAll_OnCancel()
        {
            _clearAllProps.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when clear all menu wants to clear all.
        /// </summary>
        private void ClearAll_OnConfirm()
        {
            _clearAllProps.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when ther quit menu wants to cancel.
        /// </summary>
        private void Quit_OnCancel()
        {
            _quit.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when the quit menu wants to quit.
        /// </summary>
        private void Quit_OnConfirm()
        {
            _quit.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when the new menu wants to create a prop.
        /// </summary>
        private void New_OnConfirm(string assetId)
        {
            _new.Hide();

            _place = _events.gameObject.AddComponent<PlaceObjectController>();
            _place.OnConfirm += Place_OnConfirm;
            _place.OnCancel += Place_OnCancel;
            _place.Initialize(assetId);
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _new.Hide();
            _mainMenu.Show();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="propData">The prop.</param>
        private void Place_OnConfirm(PropData propData)
        {
            _propManager
                .Active
                .Create(propData)
                .OnSuccess(ListenToProp)
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not place prop : {0}.", exception);
                });

            Object.Destroy(_place);
            _place = null;

            _splash.Show();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Place_OnConfirmController(PropController controller)
        {
            controller.Content.GameObject.transform.SetParent(null, true);

            Object.Destroy(_place);
            _place = null;

            _splash.Show();
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Place_OnCancel()
        {
            Object.Destroy(_place);
            _place = null;

            _new.Show();
        }
        
        /// <summary>
        /// Called when the controller asks to open the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Controller_OnAdjust(PropController controller)
        {
            if (null != _propAdjust)
            {
                Object.Destroy(_propAdjust.gameObject);
                _propAdjust = null;
                _propEdit = null;
            }

            // hide the splash on the controller
            controller.HideSplashMenu();

            // hide any other menus
            _splash.Hide();
            _mainMenu.Hide();

            // create a new propadjust menu
            var root = new GameObject("Prop Controls");

            _propAdjust = root.AddComponent<PropAdjustController>();
            _propAdjust.OnExit += PropAdjust_OnExit;
            _propAdjust.Initialize(controller);

            _propEdit = root.AddComponent<PropEditController>();
            _propEdit.OnMove += PropEdit_OnMove;
            _propEdit.Initialize(controller);
        }

        /// <summary>
        /// Called to move the prop.
        /// </summary>
        /// <param name="propController">The controller.</param>
        private void PropEdit_OnMove(PropController propController)
        {
            ClosePropControls();

            _place = _events.gameObject.AddComponent<PlaceObjectController>();
            _place.OnConfirmController += Place_OnConfirmController;
            _place.OnCancel += Place_OnCancel;
            _place.Initialize(propController);
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(PropController controller)
        {
            ClosePropControls();

            controller.ShowSplashMenu();

            _splash.Show();
        }
    }
}