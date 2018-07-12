using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controller for UI that edits content.
    /// </summary>
    [InjectVine("Element.Edit")]
    public class EditElementController : InjectableIUXController
    {
        /// <summary>
        /// The design controller.
        /// </summary>
        private ElementSplashDesignController _controller;

        /// <summary>
        /// Visibility prop.
        /// </summary>
        private ElementSchemaProp<bool> _isVisible;

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..tgl-visibility")]
        public ToggleWidget TglVisible { get; private set; }
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
        public event Action<ElementSplashDesignController> OnMove;

        /// <summary>
        /// Called when a reparent is requested.
        /// </summary>
        public event Action<ElementSplashDesignController> OnReparent;

        /// <summary>
        /// Called when a delete is requested.
        /// </summary>
        public event Action<ElementSplashDesignController> OnDelete;

        /// <summary>
        /// Called when a duplicate is requested.
        /// </summary>
        public event Action<ElementSplashDesignController> OnDuplicate;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(ElementSplashDesignController controller)
        {
            _controller = controller;

            _isVisible = _controller.Element.Schema.Get<bool>("visible");
            _isVisible.OnChanged += Visible_OnChanged;
            UpdateVisibilityToggle();
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnMove.Activator.OnActivated += Move_OnActivated;
            BtnReparent.Activator.OnActivated += Reparent_OnActivated;
            BtnDelete.Activator.OnActivated += Delete_OnActivated;
            BtnDuplicate.Activator.OnActivated += Duplicate_OnActivated;
            TglVisible.OnValueChanged += tgl => _isVisible.Value = tgl.Value;
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            if (null != _isVisible)
            {
                _isVisible.OnChanged -= Visible_OnChanged;
            }

            base.OnDestroy();
        }

        /// <summary>
        /// Sets the toggle value from prop.
        /// </summary>
        private void UpdateVisibilityToggle()
        {
            TglVisible.Value = _isVisible.Value;
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

        /// <summary>
        /// Called when visibility changes.
        /// </summary>
        private void Visible_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateVisibilityToggle();
        }
    }
}