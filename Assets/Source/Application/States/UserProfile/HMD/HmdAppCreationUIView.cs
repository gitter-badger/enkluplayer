using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Splash menu for users after login.
    /// </summary>
    public class HmdAppCreationUIView : MonoBehaviourIUXController
    {

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; set; }

        /// <summary>
        /// Called to back to main menu.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Called when app name is confirmed.
        /// </summary>
        public event Action<string, string> OnOk;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnOk.Activator.OnActivated += Ok_OnActivated;
            BtnCancel.Activator.OnActivated += Cancel_OnActivated;
        }

        /// <summary>
        /// Called when ok has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Ok_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnOk)
            {
                OnOk("TrialAppName", "Dummy description");
            }
        }

        /// <summary>
        /// Called when cancel has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Cancel_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}