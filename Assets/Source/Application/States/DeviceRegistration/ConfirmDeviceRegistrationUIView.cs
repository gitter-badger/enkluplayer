using System;
using System.Linq;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages.GetMyOrganizations;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Simple dialog to allow the user to confirm registration.
    /// </summary>
    public class ConfirmDeviceRegistrationUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-confirm")]
        public ButtonWidget BtnConfirm { get; set; }
        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; set; }
        [InjectElements("..menu-main")]
        public MenuWidget Menu { get; set; }
        
        /// <summary>
        /// Called to confirm device registration.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called to cancel device registration.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Populates view with message.
        /// </summary>
        public void Populate(Body[] organizations)
        {
            Menu.Schema.Set(
                "description",
                string.Format("You belong to ({0}) organization(s). Would you like to register this device?", organizations.Length));
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnConfirm.Activator.OnActivated += _ =>
            {
                if (null != OnConfirm)
                {
                    OnConfirm();
                }
            };

            BtnCancel.Activator.OnActivated += _ =>
            {
                if (null != OnCancel)
                {
                    OnCancel();
                }
            };
        }
    }
}