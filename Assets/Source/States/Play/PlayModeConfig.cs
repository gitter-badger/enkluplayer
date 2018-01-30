using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class PlayModeConfig : MonoBehaviour
    {
        public IUXEventHandler Root;
        public SplashMenuController SplashMenu;
        public MainMenuController MainMenu;
        public GameObject NewMenu;
        public ClearAllPropsController ClearAllMenu;
        public QuitController QuitMenu;
    }
}