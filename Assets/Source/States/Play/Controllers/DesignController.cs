using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class DesignController
    {
        private readonly IElementFactory _elements;

        private PlayModeConfig _playConfig;

        private IUXEventHandler _events;

        private SplashMenuController _splash;
        private MainMenuController _mainMenu;
        private ClearAllPropsController _clearAllProps;
        private QuitController _quit;

        private Float _float;

        public DesignController(IElementFactory elements)
        {
            _elements = elements;
        }

        public void Setup()
        {
            _playConfig = Object.FindObjectOfType<PlayModeConfig>();

            if (null == _playConfig)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }

            _events = _playConfig.Events;

            // create float
            _float = (Float) _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "Root"
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "Root",
                        Type = ElementTypes.FLOAT,
                        Schema = new ElementSchemaData
                        {
                            Vectors = new Dictionary<string, Vec3>
                            {
                                {
                                    "position",
                                    new Vec3(0, 0, 2)
                                }
                            }
                        }
                    }
                }
            });
            _float.GameObject.transform.parent = _events.transform;

            SetupMenus();

            _splash.Show();
        }

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
            _mainMenu.Initialize(_events, parent);

            _clearAllProps = Object.Instantiate(_playConfig.ClearAllMenu, _events.transform);
            _clearAllProps.OnCancel += ClearAll_OnCancel;
            _clearAllProps.OnConfirm += ClearAll_OnConfirm;
            _clearAllProps.Initialize(_events, parent);

            _quit = Object.Instantiate(_playConfig.QuitMenu, _events.transform);
            _quit.OnCancel += Quit_OnCancel;
            _quit.OnConfirm += Quit_OnConfirm;
            _quit.Initialize(_events, parent);
        }

        public void Teardown()
        {
            _splash.Uninitialize();
            _mainMenu.Uninitialize();
            _clearAllProps.Uninitialize();
            _quit.Uninitialize();

            Object.Destroy(_splash);
            Object.Destroy(_mainMenu);
            Object.Destroy(_clearAllProps);
            Object.Destroy(_quit);
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

        private void MainMenu_OnQuit()
        {
            _mainMenu.Hide();
            _quit.Show();
        }

        private void MainMenu_OnClearAll()
        {
            _mainMenu.Hide();
            _clearAllProps.Show();
        }

        private void ClearAll_OnCancel()
        {
            _clearAllProps.Hide();
            _mainMenu.Show();
        }

        private void ClearAll_OnConfirm()
        {
            _clearAllProps.Hide();
            _mainMenu.Show();
        }

        private void Quit_OnCancel()
        {
            _quit.Hide();
            _mainMenu.Show();
        }

        private void Quit_OnConfirm()
        {
            _quit.Hide();
            _mainMenu.Show();
        }
    }
}