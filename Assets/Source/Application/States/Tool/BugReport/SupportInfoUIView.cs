using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Support information.
    /// </summary>
    public class SupportInfoUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        /// <summary>
        /// Called when user wishes to close the menu.
        /// </summary>
        public event Action OnClose;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnOk.Activator.OnActivated += _ =>
            {
                if (null != OnClose)
                {
                    OnClose();
                }
            };
        }
    }
}