using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UI for mesh capture.
    /// </summary>
    public class MeshCaptureSplashUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; set; }

        [InjectElements("..btn-save")]
        public ButtonWidget BtnSave{ get; set; }

        [InjectElements("..btn-options")]
        public ButtonWidget BtnOptions { get; set; }

        [InjectElements("..tgl-autosave")]
        public ToggleWidget TglAutoSave { get; set; }

        /// <summary>
        /// Called when back is requested.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called when save is requested.
        /// </summary>
        public event Action OnSave;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnBack)
                {
                    OnBack();
                }
            };

            BtnSave.Activator.OnActivated += _ =>
            {
                if (null != OnSave)
                {
                    OnSave();
                }
            };

            BtnOptions.Activator.OnActivated += _ =>
            {
                // 
            };

            TglAutoSave.OnValueChanged += _ =>
            {
                if (TglAutoSave.Value)
                {
                    BtnSave.Activator.InteractionEnabled = false;
                    BtnSave.Schema.Set("ready.color", "Disabled");
                }
                else
                {
                    BtnSave.Activator.InteractionEnabled = true;
                    BtnSave.Schema.Set("ready.color", "Ready");
                }
            };
        }
    }
}