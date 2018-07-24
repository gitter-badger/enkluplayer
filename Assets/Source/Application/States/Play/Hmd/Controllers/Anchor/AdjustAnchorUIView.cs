using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for adjusting an anchor.
    /// </summary>
    public class AdjustAnchorUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Controller.
        /// </summary>
        private AnchorDesignController _controller;
        
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; private set; }
        [InjectElements("..btn-move")]
        public ButtonWidget BtnMove { get; private set; }
        [InjectElements("..btn-reload")]
        public ButtonWidget BtnReload { get; private set; }
        [InjectElements("..btn-resave")]
        public ButtonWidget BtnResave { get; private set; }
        [InjectElements("..btn-delete")]
        public ButtonWidget BtnTrash { get; private set; }
        
        /// <summary>
        /// Called when the menu should be exited.
        /// </summary>
        public event Action<AnchorDesignController> OnExit;

        /// <summary>
        /// Called when reload is requested.
        /// </summary>
        public event Action<AnchorDesignController> OnReload;

        /// <summary>
        /// Called when resave is requested.
        /// </summary>
        public event Action<AnchorDesignController> OnResave;

        /// <summary>
        /// Called when move is requested.
        /// </summary>
        public event Action<AnchorDesignController> OnMove;

        /// <summary>
        /// Called when delete is requested.
        /// </summary>
        public event Action<AnchorDesignController> OnDelete;

        /// <summary>
        /// Initializes the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        public void Initialize(AnchorDesignController controller)
        {
            _controller = controller;

            transform.position = _controller.transform.position;

            enabled = true;
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnExit)
                {
                    OnExit(_controller);
                }
            };

            BtnMove.Activator.OnActivated += _ =>
            {
                if (null != OnMove)
                {
                    OnMove(_controller);
                }
            };

            BtnReload.Activator.OnActivated += _ =>
            {
                if (null != OnReload)
                {
                    OnReload(_controller);
                }
            };

            BtnResave.Activator.OnActivated += _ =>
            {
                if (null != OnResave)
                {
                    OnResave(_controller);
                }
            };

            BtnTrash.Activator.OnActivated += _ =>
            {
                if (null != OnDelete)
                {
                    OnDelete(_controller);
                }
            };
        }
    }
}