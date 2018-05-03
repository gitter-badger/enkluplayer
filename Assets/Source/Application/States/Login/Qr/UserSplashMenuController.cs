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
        
        /// <summary>
        /// Called when an app has been selected.
        /// </summary>
        public event Action<string> OnAppSelected;

        /// <summary>
        /// Initializes the view with data.
        /// </summary>
        /// <param name="profile">Profile data.</param>
        public void Initialize(UserProfileCacheData profile)
        {
            foreach (var app in profile.Apps)
            {
                var vine = string.Format(
                    @"<?Vine><Button label='{0}' />",
                    app.Name.Replace("'", ""));
                
                var button = (ButtonWidget) Elements.Element(vine);
                button.Activator.OnActivated += AppButton_OnActivated(app.Id);
                Menu.AddChild(button);
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