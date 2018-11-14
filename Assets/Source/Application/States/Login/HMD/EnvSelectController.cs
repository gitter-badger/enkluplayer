using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Environment selection.
    /// </summary>
    public class EnvSelectController : MonoBehaviourIUXController
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        [Inject]
        public ApplicationConfig Config { get; private set; }

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu")]
        public MenuWidget Menu { get; set; }

        /// <summary>
        /// Called when environment has been selected.
        /// </summary>
        public event Action<EnvironmentData> OnEnvironmentSelected;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            foreach (var env in Config.Network.AllEnvironments)
            {
                var selection = env;
                var btn = (ButtonWidget) Elements.Element(string.Format("<Button label='{0}' />", selection.Name));
                
                btn.OnActivated += _ =>
                {
                    if (null != OnEnvironmentSelected)
                    {
                        OnEnvironmentSelected(selection);
                    }
                };

                Menu.AddChild(btn);
            }
        }
    }
}