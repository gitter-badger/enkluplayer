﻿using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class MainDesignState : IDesignState
    {
        private readonly IAdminAppController _appController;

        /// <summary>
        /// Design controller.
        /// </summary>
        private DesignController _design;

        private GameObject _unityRoot;

        private Element _dynamicRoot;

        private Element _staticRoot;

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
        
        public MainDesignState(IAdminAppController app)
        {
            _appController = app;
        }

        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _unityRoot = unityRoot;
            _dynamicRoot = dynamicRoot;
            _staticRoot = staticRoot;
            
            // splash menu
            {
                _splash = unityRoot.AddComponent<SplashMenuController>();
                _splash.enabled = false;
                _splash.OnOpenMenu += Splash_OnOpenMenu;
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

        public void Enter(object context)
        {
            _splash.enabled = true;
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            CloseAll();
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
            _design.ChangeState<ContentDesignState>();
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

            _appController.Active.DestroyAll();

            _dynamicRoot.Schema.Set("focus.visible", true);
            _mainMenu.enabled = true;
        }
    }
}