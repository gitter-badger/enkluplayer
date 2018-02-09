using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for adjusting a prop.
    /// </summary>
    [InjectVine("Prop.Adjust")]
    public class PropAdjustController : InjectableIUXController
    {
        /// <summary>
        /// Ties together the propdata and content.
        /// </summary>
        private PropController _controller;

        /// <summary>
        /// Starting scale.
        /// </summary>
        private Vector3 _startScale;

        /// <summary>
        /// Starting rotation.
        /// </summary>
        private Vector3 _startRotation;

        /// <summary>
        /// Starting position.
        /// </summary>
        private Vector3 _startPosition;

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
        [InjectElements("..btn-rotate")]
        public ButtonWidget BtnRotate{ get; private set; }
        [InjectElements("..btn-scale")]
        public ButtonWidget BtnScale { get; private set; }
        [InjectElements("..btn-x")]
        public ButtonWidget BtnX { get; private set; }
        [InjectElements("..btn-y")]
        public ButtonWidget BtnY { get; private set; }
        [InjectElements("..btn-z")]
        public ButtonWidget BtnZ { get; private set; }
        [InjectElements("..slider-x")]
        public SliderWidget SliderX { get; private set; }
        [InjectElements("..slider-y")]
        public SliderWidget SliderY { get; private set; }
        [InjectElements("..slider-z")]
        public SliderWidget SliderZ { get; private set; }
        [InjectElements("..slider-scale")]
        public SliderWidget SliderScale { get; private set; }
        [InjectElements("..slider-rotate")]
        public SliderWidget SliderRotate { get; private set; }

        /// <summary>
        /// Called when the menu should be exited.
        /// </summary>
        public event Action<PropController> OnExit;

        /// <summary>
        /// Initiailizes the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        public void Initialize(PropController controller)
        {
            _controller = controller;

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnRotate.Activator.OnActivated += BtnRotate_OnActivated;
            BtnScale.Activator.OnActivated += BtnScale_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;

            SliderX.OnUnfocused += SliderX_OnUnfocused;
            SliderY.OnUnfocused += SliderY_OnUnfocused;
            SliderZ.OnUnfocused += SliderZ_OnUnfocused;

            SliderScale.OnUnfocused += SliderScale_OnUnfocused;
            SliderRotate.OnUnfocused += SliderRotate_OnUnfocused;

            ResetMenuPosition();

            enabled = true;
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
                _controller.Content.GameObject.transform.position = SliderX.Focus.ToVector() - offset;
            }

            if (SliderY.Visible)
            {
                var offset = BtnY.GameObject.transform.position - Container.GameObject.transform.position;
                _controller.Content.GameObject.transform.position = SliderY.Focus.ToVector() - offset;
            }

            if (SliderZ.Visible)
            {
                var zScale = 10;
                _controller.Content.GameObject.transform.position =
                    _startPosition + new Vector3(0, 0, (SliderZ.Value - 0.5f) * zScale);
            }

            if (SliderScale.Visible)
            {
                const float scaleDiff = 2f;
                var start = _startScale.x;
                var val = start + (SliderScale.Value - 0.5f) * scaleDiff;
                _controller.Content.GameObject.transform.localScale = val * Vector3.one;
            }

            if (SliderRotate.Visible)
            {
                var start = _startRotation.y;
                var val = start + (SliderRotate.Value - 0.5f) * 360;
                _controller.Content.GameObject.transform.localRotation = Quaternion.Euler(0, val, 0);
            }
        }

        /// <summary>
        /// Resets the position of the menu to the center of the content.
        /// </summary>
        private void ResetMenuPosition()
        {
            transform.position = _controller.Content.GameObject.transform.position;
        }

        /// <summary>
        /// Called when the x slider has unfocused.
        /// </summary>
        private void SliderX_OnUnfocused()
        {
            SliderX.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;
        }

        /// <summary>
        /// Called when the y slider has unfocused.
        /// </summary>
        private void SliderY_OnUnfocused()
        {
            SliderY.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;
        }

        /// <summary>
        /// Called when the z slider has unfocused.
        /// </summary>
        private void SliderZ_OnUnfocused()
        {
            SliderZ.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;
        }

        /// <summary>
        /// Called when the scale slider has unfocused.
        /// </summary>
        private void SliderScale_OnUnfocused()
        {
            SliderScale.LocalVisible = false;
            Container.LocalVisible = true;
        }

        /// <summary>
        /// Called when the rotate slider has unfocused.
        /// </summary>
        private void SliderRotate_OnUnfocused()
        {
            SliderRotate.LocalVisible = false;
            Container.LocalVisible = true;
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
        /// Called when the rotate button has been activated.
        /// </summary>
        private void BtnRotate_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;

            _startRotation = _controller.Content.GameObject.transform.localRotation.eulerAngles;
            SliderRotate.LocalVisible = true;
        }

        /// <summary>
        /// Called when the scale button has been activated.
        /// </summary>
        private void BtnScale_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;

            _startScale = _controller.Content.GameObject.transform.localScale;
            SliderScale.LocalVisible = true;
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
            _startPosition = _controller.Content.GameObject.transform.position;

            Container.LocalVisible = false;
            SliderZ.LocalVisible = true;
        }
    }
}