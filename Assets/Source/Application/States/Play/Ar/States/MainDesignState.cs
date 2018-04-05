using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Entry state of design controller.
    /// </summary>
    public class MainDesignState : IArDesignState
    {
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Manages controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;

        /// <summary>
        /// Design controller.
        /// </summary>
        private ArDesignController _design;
        
        /// <summary>
        /// Root of dynamic menus.
        /// </summary>
        private Element _dynamicRoot;
        
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
        private ClearSceneController _clearScene;

        /// <summary>
        /// Distance filter.
        /// </summary>
        private readonly DistanceElementControllerFilter _distanceFilter = new DistanceElementControllerFilter();

        /// <summary>
        /// Content filter.
        /// </summary>
        private readonly TypeElementControllerFilter _contentFilter = new TypeElementControllerFilter(typeof(ContentWidget));

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainDesignState(
            IMessageRouter messages,
            IElementControllerManager controllers)
        {
            _messages = messages;
            _controllers = controllers;
        }

        /// <inheritdoc />
        public void Initialize(
            ArDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _dynamicRoot = dynamicRoot;
            
            // splash menu
            {
                _splash = unityRoot.AddComponent<SplashMenuController>();
                _splash.enabled = false;
                _splash.OnOpenMenu += Splash_OnOpenMenu;
                _splash.OnBack += Splash_OnBack;
                dynamicRoot.AddChild(_splash.Root);
            }

            // main menu
            {
                _mainMenu = unityRoot.AddComponent<MainMenuController>();
                _mainMenu.enabled = false;
                _mainMenu.OnBack += MainMenu_OnBack;
                _mainMenu.OnClearAll += MainMenu_OnClearAll;
                _mainMenu.OnNew += MainMenu_OnNew;
                _mainMenu.OnShowAnchorMenu += MainMenu_OnShowAnchorMenu;
                _mainMenu.OnPlay += MainMenu_OnPlay;
                dynamicRoot.AddChild(_mainMenu.Root);
            }

            // clear props menu
            {
                _clearScene = unityRoot.AddComponent<ClearSceneController>();
                _clearScene.enabled = false;
                _clearScene.OnCancel += ClearAll_OnCancel;
                _clearScene.OnConfirm += ClearAll_OnConfirm;
                dynamicRoot.AddChild(_clearScene.Root);
            }
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            UnityEngine.Object.Destroy(_splash);
            UnityEngine.Object.Destroy(_mainMenu);
            UnityEngine.Object.Destroy(_clearScene);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            _controllers
                .Filter(_distanceFilter)
                .Filter(_contentFilter)
                .Add<ContentDesignController>(
                    new ContentDesignController.ContentDesignControllerContext
                    {
                        Delegate = _design.Elements,
                        OnAdjust = Content_OnAdjust
                    });

            _splash.enabled = true;
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _controllers
                .Remove<ContentDesignController>()
                .Unfilter(_contentFilter)
                .Unfilter(_distanceFilter);

            CloseAll();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _splash.enabled = false;
            _mainMenu.enabled = false;
            _clearScene.enabled = false;

            CloseAllPropControllerSplashes();
        }
        
        /// <summary>
        /// Closes all splash menus.
        /// </summary>
        private void CloseAllPropControllerSplashes()
        {
            var designControllers = new List<ContentDesignController>();
            _controllers.All(designControllers);

            for (var i = 0; i < designControllers.Count; i++)
            {
                designControllers[i].HideSplashMenu();
            }
        }

        /// <summary>
        /// Called when the splash menu wants to go back.
        /// </summary>
        private void Splash_OnBack()
        {
            _messages.Publish(MessageTypes.USER_PROFILE);
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
        /// Called when main menu wants to clear all props.
        /// </summary>
        private void MainMenu_OnClearAll()
        {
            _mainMenu.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", false);
            _clearScene.enabled = true;
        }

        /// <summary>
        /// Called when main menu wants to create a new prop.
        /// </summary>
        private void MainMenu_OnNew()
        {
            _mainMenu.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", false);

            // content design!
            _design.ChangeState<NewContentDesignState>();
        }

        /// <summary>
        /// Called when the main menu wants to display the anchor menu.
        /// </summary>
        private void MainMenu_OnShowAnchorMenu()
        {
            _mainMenu.enabled = false;
            CloseAllPropControllerSplashes();

            // anchor design!
            _design.ChangeState<AnchorDesignState>();
        }

        /// <summary>
        /// Move to play mode.
        /// </summary>
        private void MainMenu_OnPlay()
        {
            CloseAll();
            _dynamicRoot.Schema.Set("visible", false);
        }

        /// <summary>
        /// Called when clear all menu wants to cancel.
        /// </summary>
        private void ClearAll_OnCancel()
        {
            _clearScene.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }

        /// <summary>
        /// Called when clear all menu wants to clear all.
        /// </summary>
        private void ClearAll_OnConfirm()
        {
            _clearScene.enabled = false;

            _design
                .Elements
                .DestroyAll()
                .OnSuccess(_ =>
                {
                    _mainMenu.enabled = true;
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not destroy all elements in active scene : {0}.",
                        exception);
                });

            _dynamicRoot.Schema.Set("focus.visible", true);
        }

        /// <summary>
        /// Called when content requests an adjustment.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void Content_OnAdjust(ContentDesignController controller)
        {
            _design.ChangeState<EditContentDesignState>(controller);
        }
    }
}