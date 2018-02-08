using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls clear all props menu.
    /// </summary>
    [InjectVine("Design.ClearAllProps")]
    public class ClearAllPropsController : InjectableIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-yes")]
        public ButtonWidget BtnYes { get; private set; }

        [InjectElements("..btn-no")]
        public ButtonWidget BtnNo { get; private set; }

        /// <summary>
        /// Called when we wish to confirm clearing all props.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called when we wish to cancel clearing all props.
        /// </summary>
        public event Action OnCancel;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnYes.Activator.OnActivated += _ =>
            {
                if (null != OnConfirm)
                {
                    OnConfirm();
                }
            };

            BtnNo.Activator.OnActivated += _ =>
            {
                if (null != OnCancel)
                {
                    OnCancel();
                }
            };
        }
    }
}