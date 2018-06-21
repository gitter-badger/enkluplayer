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
        [InjectElements("..btn-setparent")]
        public ButtonWidget BtnReparent { get; private set; }
        [InjectElements("..btn-delete")]
        public ButtonWidget BtnDelete { get; private set; }
        [InjectElements("..btn-duplicate")]
        public ButtonWidget BtnDuplicate { get; private set; }
        
        /// <summary>
        /// Called when a move is requested.
        /// </summary>
        public event Action<ContentDesignController> OnMove;

        /// <summary>
        /// Called when a reparent is requested.
        /// </summary>
        public event Action<ContentDesignController> OnReparent;

        /// <summary>
        /// Called when a delete is requested.
        /// </summary>
        public event Action<ContentDesignController> OnDelete;

        /// <summary>
        /// Called when a duplicate is requested.
        /// </summary>
        public event Action<ContentDesignController> OnDuplicate;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(ContentDesignController controller)
        {
            _controller = controller;
            
            BtnMove.Activator.OnActivated += Move_OnActivated;
            BtnReparent.Activator.OnActivated += Reparent_OnActivated;
            BtnDelete.Activator.OnActivated += Delete_OnActivated;
            BtnDuplicate.Activator.OnActivated += Duplicate_OnActivated;
        }

        /// <summary>
        /// Called when the duplicate button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void Duplicate_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnDuplicate)
            {
                OnDuplicate(_controller);
            }
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
        /// Called when the reparent button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Reparent_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnReparent)
            {
                OnReparent(_controller);
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