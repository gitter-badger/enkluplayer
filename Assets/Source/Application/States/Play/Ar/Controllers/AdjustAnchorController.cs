using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for adjusting an anchor.
    /// </summary>
    [InjectVine("Anchor.Adjust")]
    public class AdjustAnchorController : InjectableIUXController
    {
        /// <summary>
        /// Controller.
        /// </summary>
        private AnchorDesignController _controller;

        /// <summary>
        /// Starting position.
        /// </summary>
        private Vector3 _startPosition;

        /// <summary>
        /// Starting forward.
        /// </summary>
        private Vector3 _startForward;
        
        /// <summary>
        /// Manages intentions.
        /// </summary>
        [Inject]
        public IIntentionManager Intention { get; set; }

        /// <summary>
        /// The container for all the control buttons.
        /// </summary>
        [InjectElements("..controls")]
        public Widget Container { get; private set; }

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; private set; }
        [InjectElements("..btn-x")]
        public ButtonWidget BtnX { get; private set; }
        [InjectElements("..btn-y")]
        public ButtonWidget BtnY { get; private set; }
        [InjectElements("..btn-z")]
        public ButtonWidget BtnZ { get; private set; }
        [InjectElements("..btn-delete")]
        public ButtonWidget BtnTrash { get; private set; }
        [InjectElements("..slider-x")]
        public SliderWidget SliderX { get; private set; }
        [InjectElements("..slider-y")]
        public SliderWidget SliderY { get; private set; }
        [InjectElements("..slider-z")]
        public SliderWidget SliderZ { get; private set; }

        /// <summary>
        /// Called when the menu should be exited.
        /// </summary>
        public event Action<AnchorDesignController> OnExit;

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
            
            ResetMenuPosition();

            enabled = true;
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;
            BtnTrash.Activator.OnActivated += BtnTrash_OnActivated;

            SliderX.OnUnfocused += SliderX_OnUnfocused;
            SliderY.OnUnfocused += SliderY_OnUnfocused;
            SliderZ.OnUnfocused += SliderZ_OnUnfocused;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            if (null == _controller)
            {
                return;
            }

            if (SliderX.Visible)
            {
                var offset = BtnX.GameObject.transform.position - Container.GameObject.transform.position;

                var O = Container.GameObject.transform.position;
                var d = Intention.Right.ToVector();

                // project focus onto line
                var focus = SliderX.Focus.ToVector();
                var scalar = Vector3.Dot(focus - O, d);
                var diff = scalar * d;

                _controller.transform.position = O + diff - offset;
            }

            if (SliderY.Visible)
            {
                var offset = BtnY.GameObject.transform.position - Container.GameObject.transform.position;

                var O = Container.GameObject.transform.position;
                var d = Intention.Up.ToVector();

                // project focus onto line
                var focus = SliderY.Focus.ToVector();
                var scalar = Vector3.Dot(focus - O, d);
                var diff = scalar * d;

                _controller.transform.position = O + diff - offset;
            }

            if (SliderZ.Visible)
            {
                var zScale = 10;
                var scalar = (SliderZ.Value - 0.5f) * zScale;
                _controller.transform.position = _startPosition + scalar * _startForward;
            }
        }

        /// <summary>
        /// Resets the position of the menu to the center of the content.
        /// </summary>
        private void ResetMenuPosition()
        {
            transform.position = _controller.transform.position;
        }

        /// <summary>
        /// Called when the x slider has unfocused.
        /// </summary>
        private void SliderX_OnUnfocused()
        {
            SliderX.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;

            _controller.FinalizeEdit();
        }

        /// <summary>
        /// Called when the y slider has unfocused.
        /// </summary>
        private void SliderY_OnUnfocused()
        {
            SliderY.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;

            _controller.FinalizeEdit();
        }

        /// <summary>
        /// Called when the z slider has unfocused.
        /// </summary>
        private void SliderZ_OnUnfocused()
        {
            SliderZ.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;

            _controller.FinalizeEdit();
        }
        
        /// <summary>
        /// Called when the back button has been activated.
        /// </summary>
        private void BtnBack_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnExit)
            {
                OnExit(_controller);
            }
        }
        
        /// <summary>
        /// Called when the x button has been activated.
        /// </summary>
        private void BtnX_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
            SliderX.LocalVisible = true;
        }

        /// <summary>
        /// Called when the y button has been activated.
        /// </summary>
        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
            SliderY.LocalVisible = true;
        }

        /// <summary>
        /// Called when the z button has been activated.
        /// </summary>
        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            _startPosition = _controller.transform.position;
            _startForward = Intention.Forward.ToVector();

            Container.LocalVisible = false;
            SliderZ.LocalVisible = true;
        }

        /// <summary>
        /// Called when the delete button has been activated.
        /// </summary>
        private void BtnTrash_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnDelete)
            {
                OnDelete(_controller);
            }
        }
    }
}