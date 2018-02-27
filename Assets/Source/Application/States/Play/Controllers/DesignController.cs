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
        private readonly IAppController _appController;

        /// <summary>
        /// Voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;
        
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
        /// Anchor menu.
        /// </summary>
        private AnchorMenuController _anchors;

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
        /// Root float.
        /// </summary>
        private FloatWidget _float;

        /// <summary>
        /// Parent container.
        /// </summary>
        private ScaleTransition _parent;

        /// <summary>
        /// Root for static menus.
        /// </summary>
        private GameObject _staticRoot;

        /// <summary>
        /// Constuctor.
        /// </summary>
        public DesignController(
            IElementFactory elements,
            IAppController appController,
            IVoiceCommandManager voice)
        {
            _elements = elements;
            _appController = appController;
            _voice = voice;
        }

        /// <summary>
        /// Starts controllers.
        /// </summary>
        public void Setup(PlayModeConfig config)
        {
            _playConfig = config;
            _events = _playConfig.Events;

            _voice.Register("design", Voice_OnDesignCommand);
            
            // create float
            _float = (FloatWidget) _elements.Element(@"<?Vine><Float id='Root' position=(0, 0, 2) face='camera'><ScaleTransition /></Float>");
            _float.GameObject.transform.parent = _events.transform;
            _parent = (ScaleTransition) _float.Children[0];
            
            SetupMenus();
            Start();
        }

        /// <summary>
        /// Tears down all controllers and destroys them.
        /// </summary>
        public void Teardown()
        {
            Object.Destroy(_events.gameObject);
        }

        /// <summary>
        /// Creates and initializes all menus.
        /// </summary>
        private void SetupMenus()
        {
            _splash = _events.gameObject.AddComponent<SplashMenuController>();
            _splash.enabled = false;
            _splash.OnOpenMenu += Splash_OnOpenMenu;
            _parent.AddChild(_splash.Root);
            
            _mainMenu = _events.gameObject.AddComponent<MainMenuController>();
            _mainMenu.enabled = false;
            _mainMenu.OnBack += MainMenu_OnBack;
            _mainMenu.OnQuit += MainMenu_OnQuit;
            _mainMenu.OnClearAll += MainMenu_OnClearAll;
            _mainMenu.OnNew += MainMenu_OnNew;
            _mainMenu.OnShowAnchors += MainMenu_OnShowAnchors;
            _mainMenu.OnPlay += MainMenu_OnPlay;
            _parent.AddChild(_mainMenu.Root);

            _anchors = _events.gameObject.AddComponent<AnchorMenuController>();
            _anchors.enabled = false;
            _anchors.OnBack += Anchors_OnBack;
            _anchors.OnNew += Anchors_OnNew;
            _anchors.OnShowChildrenChanged += Anchors_OnShowChildrenChanged;
            
            _clearAllProps = _events.gameObject.AddComponent<ClearAllPropsController>();
            _clearAllProps.enabled = false;
            _clearAllProps.OnCancel += ClearAll_OnCancel;
            _clearAllProps.OnConfirm += ClearAll_OnConfirm;
            _parent.AddChild(_clearAllProps.Root);

            _quit = _events.gameObject.AddComponent<QuitController>();
            _quit.enabled = false;
            _quit.OnCancel += Quit_OnCancel;
            _quit.OnConfirm += Quit_OnConfirm;
            _parent.AddChild(_quit.Root);

            _new = _events.gameObject.AddComponent<NewItemController>();
            _new.enabled = false;
            _new.OnCancel += New_OnCancel;
            _new.OnConfirm += New_OnConfirm;
            _parent.AddChild(_new.Root);

            _staticRoot = new GameObject("Static Menus");

            _place = _staticRoot.AddComponent<PlaceObjectController>();
            _place.OnConfirm += Place_OnConfirm;
            _place.OnConfirmController += Place_OnConfirmController;
            _place.OnCancel += Place_OnCancel;
            _place.enabled = false;

            _propAdjust = _staticRoot.AddComponent<PropAdjustController>();
            _propAdjust.OnExit += PropAdjust_OnExit;
            _propAdjust.enabled = false;
            _propAdjust.SliderRotate.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
            _propAdjust.SliderX.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
            _propAdjust.SliderY.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
            _propAdjust.SliderZ.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;

            _propEdit = _staticRoot.AddComponent<PropEditController>();
            _propEdit.OnMove += PropEdit_OnMove;
            _propEdit.OnDelete += PropEdit_OnDelete;
            _propEdit.enabled = false;
        }

        /// <summary>
        /// Closes prop menus.
        /// </summary>
        private void ClosePropControls()
        {
            _propAdjust.enabled = false;
            _propEdit.enabled = false;
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _propAdjust.enabled = false;
            _propEdit.enabled = false;
            _splash.enabled = false;
            _mainMenu.enabled = false;
            _anchors.enabled = false;
            _clearAllProps.enabled = false;
            _quit.enabled = false;
            _place.enabled = false;
            _new.enabled = false;

            CloseAllPropControllerSplashes();
        }

        /// <summary>
        /// Closes all PropController splashes
        /// </summary>
        private void CloseAllPropControllerSplashes()
        {
            foreach (var prop in _appController.Active.ContentControllers)
            {
                prop.HideSplashMenu();
            }
        }

        /// <summary>
        /// Opens all PropController splashes
        /// </summary>
        private void OpenAllPropControllerSplashes()
        {
            foreach (var prop in _appController.Active.ContentControllers)
            {
                prop.ShowSplashMenu();
            }
        }

        /// <summary>
        /// Listens to prop.
        /// </summary>
        /// <param name="controller">The PropController.</param>
        private void ListenToProp(ContentDesignController controller)
        {
            controller.OnAdjust += Controller_OnAdjust;
        }

        /// <summary>
        /// Starts design mode after everything is ready.
        /// </summary>
        private void Start()
        {
            // listen to all prop controllers
            var controllers = _appController.Active.ContentControllers;
            for (int i = 0, len = controllers.Count; i < len; i++)
            {
                ListenToProp(controllers[i]);
            }

            _splash.enabled = true;
        }

        /// <summary>
        /// Called when voice command is detected.
        /// </summary>
        /// <param name="command">The detected command.</param>
        private void Voice_OnDesignCommand(string command)
        {
            CloseAll();
            _splash.enabled = true;

            _float.Schema.Set("visible", true);

            OpenAllPropControllerSplashes();
        }

        /// <summary>
        /// Called when splash wants to open the menu.
        /// </summary>
        private void Splash_OnOpenMenu()
        {
            _splash.enabled = false;
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when main menu wants to go back.
        /// </summary>
        private void MainMenu_OnBack()
        {
            _mainMenu.enabled = false;
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when main menu wants to quit.
        /// </summary>
        private void MainMenu_OnQuit()
        {
            _mainMenu.enabled = false;
            _float.Schema.Set("focus.visible", false);
            _quit.enabled = true;
        }

        /// <summary>
        /// Called when main menu wants to clear all props.
        /// </summary>
        private void MainMenu_OnClearAll()
        {
            _mainMenu.enabled = false;
            _float.Schema.Set("focus.visible", false);
            _clearAllProps.enabled = true;
        }

        /// <summary>
        /// Called when main menu wants to create a new prop.
        /// </summary>
        private void MainMenu_OnNew()
        {
            _mainMenu.enabled = false;
            _float.Schema.Set("focus.visible", false);
            _new.enabled = true;
        }

        /// <summary>
        /// Called when the main menu wants to display anchors.
        /// </summary>
        private void MainMenu_OnShowAnchors()
        {
            _mainMenu.enabled = false;
            _anchors.enabled = true;

            foreach (var controller in _appController.Active.ContentControllers)
            {
                
            }
        }

        /// <summary>
        /// Move to play mode.
        /// </summary>
        private void MainMenu_OnPlay()
        {
            CloseAll();
            _float.Schema.Set("visible", false);
        }

        /// <summary>
        /// Called when show children option has changed on anchors.
        /// </summary>
        /// <param name="value">The value.</param>
        private void Anchors_OnShowChildrenChanged(bool value)
        {
            _appController.Active.ShowAnchorChildren = value;
        }

        /// <summary>
        /// Called when new anchor is requested.
        /// </summary>
        private void Anchors_OnNew()
        {
            
        }

        /// <summary>
        /// Called when back button is pressed on anchor menu.
        /// </summary>
        private void Anchors_OnBack()
        {
            _anchors.enabled = false;
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when clear all menu wants to cancel.
        /// </summary>
        private void ClearAll_OnCancel()
        {
            _clearAllProps.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when clear all menu wants to clear all.
        /// </summary>
        private void ClearAll_OnConfirm()
        {
            _clearAllProps.enabled = false;

            _appController.Active.DestroyAll();

            _float.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when ther quit menu wants to cancel.
        /// </summary>
        private void Quit_OnCancel()
        {
            _quit.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when the quit menu wants to quit.
        /// </summary>
        private void Quit_OnConfirm()
        {
            _quit.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when the new menu wants to create an element.
        /// </summary>
        private void New_OnConfirm(string assetId)
        {
            _new.enabled = false;

            _place.Initialize(assetId);
            _place.enabled = true;
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _new.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="propData">The prop.</param>
        private void Place_OnConfirm(ElementData propData)
        {
            _appController
                .Active
                .Create(propData)
                .OnSuccess(ListenToProp)
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not place prop : {0}.", exception);
                });

            _place.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Place_OnConfirmController(ContentDesignController controller)
        {
            controller.transform.SetParent(null, true);
            controller.ShowSplashMenu();
            controller.EnableUpdates();
            
            _place.enabled = false;
            _float.Schema.Set("focus.visible", true);
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Place_OnCancel()
        {
            _place.enabled = false;

            _new.enabled = true;
        }
        
        /// <summary>
        /// Called when the controller asks to open the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Controller_OnAdjust(ContentDesignController controller)
        {
            // hide the splash on the controller
            controller.HideSplashMenu();
            
            // hide any other menus
            _splash.enabled = false;
            _mainMenu.enabled = false;

            _float.Schema.Set("focus.visible", false);

            _propAdjust.Initialize(controller);
            _propAdjust.enabled = true;
            
            _propEdit.Initialize(controller);
            _propEdit.enabled = true;
        }

        /// <summary>
        /// Called to move the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void PropEdit_OnMove(ContentDesignController elementController)
        {
            ClosePropControls();

            elementController.HideSplashMenu();
            elementController.DisableUpdates();
            
            _place.Initialize(elementController);
            _place.enabled = true;
        }

        /// <summary>
        /// Called to delete the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void PropEdit_OnDelete(ContentDesignController elementController)
        {
            ClosePropControls();
            
            _appController.Active.Destroy(elementController.Element.Id);

            _float.Schema.Set("focus.visible", true);
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(ContentDesignController controller)
        {
            ClosePropControls();

            controller.ShowSplashMenu();

            _float.Schema.Set("focus.visible", true);
            _splash.enabled = true;
        }

        /// <summary>
        /// Called when prop adjust control visibility is changed.
        /// </summary>
        /// <param name="interactable">Interactable.</param>
        private void PropAdjustControl_OnVisibilityChanged(IInteractable interactable)
        {
            _propEdit.enabled = !interactable.Visible;
        }
    }
}