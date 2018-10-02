using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls main menu in play mode.
    /// </summary>
    public class PlayMenuUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; set; }
        [InjectElements("..btn-edit")]
        public ButtonWidget BtnEdit { get; set; }
        [InjectElements("..txt-name")]
        public CaptionWidget TxtName { get; set; }

        /// <summary>
        /// Called to go back.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called to play.
        /// </summary>
        public event Action OnEdit;

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

            BtnEdit.Activator.OnActivated += _ =>
            {
                if (null != OnEdit)
                {
                    OnEdit();
                }
            };
        }
    }
}