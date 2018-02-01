using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
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
        /// Container for all.
        /// </summary>
        private FloatWidget _float;

        /// <summary>
        /// Constuctor.
        /// </summary>
        public DesignController(IElementFactory elements)
        {
            _elements = elements;
        }

        /// <summary>
        /// Starts controllers.
        /// </summary>
        public void Setup(PlayModeConfig config)
        {
            _playConfig = config;
            _events = _playConfig.Events;
            
            // create float
            _float = (FloatWidget) _elements.Element(new ElementDescription
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
                                    new Vec3(0, 0, 1)
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

            // load asset and place
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _new.Hide();
            _mainMenu.Show();
        }
    }
}