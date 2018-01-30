using System;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class DesignController
    {
        private PlayModeConfig _playConfig;

        private IUXEventHandler _events;
        private SplashMenuController _splash;
        private MainMenuController _mainMenu;
        
        public void Setup()
        {
            _playConfig = Object.FindObjectOfType<PlayModeConfig>();

            if (null == _playConfig)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }

            _events = _playConfig.Root;

            _splash = Object.Instantiate(_playConfig.SplashMenu, _events.transform);
            _splash.OnOpenMenu += Splash_OnOpenMenu;
            _splash.Initialize(_events);

            _mainMenu = Object.Instantiate(_playConfig.MainMenu, _events.transform);
            _mainMenu.OnBack += MainMenu_OnBack;
            _mainMenu.Initialize(_events);

            _splash.Show();
        }

        public void Teardown()
        {
            _splash.Uninitialize();
            _mainMenu.Uninitialize();

            Object.Destroy(_splash);
            Object.Destroy(_events);
        }

        private void Splash_OnOpenMenu()
        {
            _splash.Hide();
            _mainMenu.Show();
        }

        private void MainMenu_OnBack()
        {
            _mainMenu.Hide();
            _splash.Show();
        }
    }
}