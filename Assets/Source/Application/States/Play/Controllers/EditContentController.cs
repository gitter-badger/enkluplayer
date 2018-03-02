using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controller for UI that edits content.
    /// </summary>
    [InjectVine("Content.Edit")]
    public class EditContentController : InjectableIUXController
    {
        /// <summary>
        /// The design controller.
        /// </summary>
        private ContentDesignController _controller;
        
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-move")]
        public ButtonWidget BtnMove { get; private set; }

        [InjectElements("..btn-delete")]
        public ButtonWidget BtnDelete { get; private set; }

        [InjectElements("..toggle-fade")]
        public ToggleWidget ToggleFade { get; private set; }
        
        /// <summary>
        /// Called when a move is requested.
        /// </summary>
        public event Action<ContentDesignController> OnMove;

        /// <summary>
        /// Called when a delete is requested.
        /// </summary>
        public event Action<ContentDesignController> OnDelete;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(ContentDesignController controller)
        {
            _controller = controller;
            
            BtnMove.Activator.OnActivated += Move_OnActivated;
            BtnDelete.Activator.OnActivated += Delete_OnActivated;
        }
        
        /// <summary>
        /// Called when the move button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void Move_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnMove)
            {
                OnMove(_controller);
            }
        }

        /// <summary>
        /// Called when the delete button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void Delete_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnDelete)
            {
                OnDelete(_controller);
            }
        }
    }
}