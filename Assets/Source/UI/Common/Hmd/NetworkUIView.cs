using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Debug UI that displays Network related information.
    /// </summary>
    public class NetworkUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject] 
        public ApplicationConfig Config { get; set; }
        
        [Inject]
        public AwsPingController PingController { get; set; }
        
        /// <summary>
        /// Injected Elements.
        /// </summary>
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }
        
        [InjectElements("..txt-config")]
        public TextWidget TxtConfig { get; set; }
        
        [InjectElements("..txt-network")]
        public TextWidget TxtNetwork { get; set; }
        
        [InjectElements("..txt-ping")]
        public TextWidget TxtPing { get; set; }

        /// <summary>
        /// Invoked when the UI should close.
        /// </summary>
        public event Action OnClose;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnClose.OnActivated += widget =>
            {
                if (OnClose != null)
                {
                    OnClose();
                }
            };
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        private void Update()
        {
            TxtConfig.Label = "Config Online: " + !Config.Network.Offline;
            TxtNetwork.Label = "Network Online: " + PingController.Online;
            TxtPing.Label = "Ping: " + (int) PingController.PingMs;
        }
    }
}