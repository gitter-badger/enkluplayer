using System;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages.GetMyApps;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Splash menu for users after login.
    /// </summary>
    public class HmdAppSelectionUIView : MonoBehaviourIUXController, IAppSelectionUIView
    {
        /// <summary>
        /// Backing variable for Apps property.
        /// </summary>
        private Body[] _apps;
        
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu-user")]
        public MenuWidget Menu { get; set; }

        /// <inheritdoc />
        public Body[] Apps
        {
            get { return _apps; }
            set
            {
                _apps = value ?? new Body[0];

                // clear previous options
                var children = Menu.Children;
                for (var i = 0; i < children.Count; i++)
                {
                    var button = children[i] as ButtonWidget;
                    if (null != button
                        && button.Schema.Get<string>("tag").Value == "app")
                    {
                        Menu.RemoveChild(button);
                    }
                }

                // add new option
                for (var i = 0; i < _apps.Length; i++)
                {
                    var app = _apps[i];
                    var vine = string.Format(
                        @"<?Vine><Button label='{0}' tag='app' />",
                        app.Name.Replace("'", ""));

                    var button = (ButtonWidget) Elements.Element(vine);
                    button.Activator.OnActivated += AppButton_OnActivated(app.Id);
                    Menu.AddChild(button);
                }
            }
        }

        /// <inheritdoc />
        public event Action<string> OnAppSelected;
        
        /// <inheritdoc />
        public event Action OnSignOut;

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