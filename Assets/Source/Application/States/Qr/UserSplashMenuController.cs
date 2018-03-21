using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Splash menu for users after login.
    /// </summary>
    [InjectVine("User.Splash")]
    public class UserSplashMenuController : InjectableIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu-user")]
        public MenuWidget Menu { get; set; }
        
        [InjectElements("..btn-worldscan")]
        public ButtonWidget BtnWorldScan { get; set; }
        
        /// <summary>
        /// Called when an app has been selected.
        /// </summary>
        public event Action<string> OnAppSelected;

        /// <summary>
        /// Called when world scan button is activated..
        /// </summary>
        public event Action OnWorldScan;

        /// <summary>
        /// Initializes the view with data.
        /// </summary>
        /// <param name="apps">App data.</param>
        public void Initialize(Trellis.Messages.GetMyApps.Body[] apps)
        {
            foreach (var app in apps)
            {
                var vine = string.Format(
                    @"<?Vine><Button label='{0}' />",
                    app.Name.Replace("'", ""));
                
                var button = (ButtonWidget) Elements.Element(vine);
                button.Activator.OnActivated += AppButton_OnActivated(app.Id);
                Menu.AddChild(button);
            }
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            
            BtnWorldScan.Activator.OnActivated += WorldScan_OnActivated;
        }
        
        /// <summary>
        /// Called when the worldscan button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void WorldScan_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnWorldScan)
            {
                OnWorldScan();
            }
        }

        /// <summary>
        /// Called when app button has been pressed.
        /// </summary>
        /// <param name="id">Id of the app.</param>
        /// <returns></returns>
        private Action<ActivatorPrimitive> AppButton_OnActivated(string id)
        {
            return primitive =>
            {
                if (null != OnAppSelected)
                {
                    OnAppSelected(id);
                }
            };
        }
    }
}