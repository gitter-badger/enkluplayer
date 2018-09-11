using System;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for adjusting a prop.
    /// </summary>
    public class AdjustElementUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Ties together the propdata and content.
        /// </summary>
        private ElementSplashDesignController _controller;

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
        /// Forward at start.
        /// </summary>
        private Vector3 _startForward;
        
        /// <summary>
        /// Flag to determine the focus on line.
        /// </summary>
        private bool _transformChangeConfirmed;

        /// <summary>
        /// Offset to add to slider positioning.
        /// </summary>
        private Vector3 _sliderOffset;
        
        /// <summary>
        /// Storing previous values before adjusting transform.
        /// </summary>
        private Vector3 _prevPosition;
        private Quaternion _prevRotation;
        private Vector3 _prevScale;
        
        /// <summary>
        /// Manages intentions.
        /// </summary>
        [Inject]
        public IIntentionManager Intention { get; set; }

        /// <summary>
        /// Loads assets.
        /// </summary>
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..controls")]
        public Widget Container { get; private set; }
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
        public event Action<ElementSplashDesignController> OnExit;
        
        /// <summary>
        /// Initiailizes the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        public void Initialize(ElementSplashDesignController controller)
        {
            _controller = controller;

            var assetSrcId = _controller.Element.Schema.Get<string>("assetSrc").Value;
            var assetData = Assets.Manifest.Data(assetSrcId);
            if (null != assetData)
            {
                var boundsData = assetData.Stats.Bounds;
                var bounds = new Bounds
                {
                    max = boundsData.Max.ToVector(),
                    min = boundsData.Min.ToVector()
                };

                var outline = _controller.gameObject.GetComponent<ModelLoadingOutline>();
                if (null == outline)
                {
                    outline = _controller.gameObject.AddComponent<ModelLoadingOutline>();
                }
                outline.Init(bounds);
            }

            ResetMenuPosition();
            enabled = true;
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnRotate.Activator.OnActivated += BtnRotate_OnActivated;
            BtnScale.Activator.OnActivated += BtnScale_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;
            
            SliderX.OnSliderValueConfirmed += SliderX_OnSliderValueConfirmed;
            SliderY.OnSliderValueConfirmed += SliderY_OnSliderValueConfirmed;
            SliderZ.OnSliderValueConfirmed += SliderZ_OnSliderValueConfirmed;
            SliderScale.OnSliderValueConfirmed += SliderScale_OnSliderValueConfirmed;
            SliderRotate.OnSliderValueConfirmed += SliderRotate_OnSliderValueConfirmed;
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
                _controller.ElementTransform.position = SliderX.Focus.ToVector() + _sliderOffset;
            }

            if (SliderY.Visible)
            {
                _controller.ElementTransform.position = SliderY.Focus.ToVector() + _sliderOffset;
            }
            
            if (SliderZ.Visible)
            {
                //_controller.ElementTransform.position = SliderZ.Focus.ToVector() + _sliderOffset;
            }

            if (SliderScale.Visible)
            {
                const float scaleDiff = 2f;
                var start = _startScale.x;
                var val = start + (SliderScale.Value - 0.5f) * scaleDiff;
                _controller.ElementTransform.localScale = val * Vector3.one;
            }

            if (SliderRotate.Visible)
            {
                var start = _startRotation.y;
                var val = start + (SliderRotate.Value - 0.5f) * 360;
                _controller.ElementTransform.localRotation = Quaternion.Euler(0, val, 0);
            }
        }

        /// <summary>
        /// Resets the position of the menu to the center of the element.
        /// </summary>
        private void ResetMenuPosition()
        {
            transform.position = _controller.ElementTransform.position;
        }

        /// <summary>
        /// Resets the position of the asset to previous position.
        /// </summary>
        private void ResetAssetTransform()
        {
            if (_transformChangeConfirmed)
            {
                return;
            }
            _controller.transform.position = _prevPosition;
            _controller.transform.localRotation = _prevRotation;
            _controller.transform.localScale = _prevScale;
        }

        /// <summary>
        /// Copy prev transform values before change.
        /// </summary>
        private void CopyCurrentAssetTransform()
        {
            _prevPosition = _controller.ElementTransform.localPosition;
            _prevRotation = _controller.ElementTransform.localRotation;
            _prevScale = _controller.ElementTransform.localScale;
        }

        /// <summary>
        /// Called when the x slider has unfocused.
        /// </summary>
        private void SliderX_OnUnfocused()
        {
            SliderX.LocalVisible = false;
            Container.LocalVisible = true;

            ResetAssetTransform();
            ResetMenuPosition();
            
            _controller.FinalizeState();
        }

        /// <summary>
        /// Called when the y slider has unfocused.
        /// </summary>
        private void SliderY_OnUnfocused()
        {
            SliderY.LocalVisible = false;
            Container.LocalVisible = true;

            ResetAssetTransform();
            ResetMenuPosition();

            _controller.FinalizeState();
        }

        /// <summary>
        /// Called when the z slider has unfocused.
        /// </summary>
        private void SliderZ_OnUnfocused()
        {
            SliderZ.LocalVisible = false;
            Container.LocalVisible = true;

            ResetAssetTransform();
            ResetMenuPosition();
            
            _controller.FinalizeState();
        }

        /// <summary>
        /// Called when the scale slider has unfocused.
        /// </summary>
        private void SliderScale_OnUnfocused()
        {
            SliderScale.LocalVisible = false;
            Container.LocalVisible = true;

            ResetAssetTransform();
            ResetMenuPosition();

            _controller.FinalizeState();
        }

        /// <summary>
        /// Called when the rotate slider has unfocused.
        /// </summary>
        private void SliderRotate_OnUnfocused()
        {
            SliderRotate.LocalVisible = false;
            Container.LocalVisible = true;

            ResetAssetTransform();
            ResetMenuPosition();

            _controller.FinalizeState();
        }

        /// <summary>
        /// Called when the back button has been activated.
        /// </summary>
        private void BtnBack_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnExit)
            {
                Destroy(_controller.gameObject.GetComponent<ModelLoadingOutline>());
                OnExit(_controller);
            }
        }

        /// <summary>
        /// Called when the rotate button has been activated.
        /// </summary>
        private void BtnRotate_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            CopyCurrentAssetTransform();
            _transformChangeConfirmed = false;

            Container.LocalVisible = false;

            _startRotation = _controller.ElementTransform.localRotation.eulerAngles;
            SliderRotate.LocalVisible = true;
        }

        /// <summary>
        /// Called when the scale button has been activated.
        /// </summary>
        private void BtnScale_OnActivated(ActivatorPrimitive activatorPrimitive) 
        {
            CopyCurrentAssetTransform();
            _transformChangeConfirmed = false;

            Container.LocalVisible = false;

            _startScale = _controller.ElementTransform.localScale;
            SliderScale.LocalVisible = true;
        }
        
        /// <summary>
        /// Called when the x button has been activated.
        /// </summary>
        private void BtnX_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            CopyCurrentAssetTransform();
            _transformChangeConfirmed = false;

            Container.LocalVisible = false;

            // center slider on element's position
            var elementPosition = _controller.ElementTransform.position;
            _sliderOffset = elementPosition - activatorPrimitive.GameObject.transform.position;
            SliderX.GameObject.transform.position = elementPosition;
            SliderX.Value = 0f;
            SliderX.Focus = (_controller.ElementTransform.position - _sliderOffset).ToVec();
            SliderX.LocalVisible = true;
        }

        /// <summary>
        /// Called when the y button has been activated.
        /// </summary>
        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            CopyCurrentAssetTransform();
            _transformChangeConfirmed = false;

            Container.LocalVisible = false;

            // center slider on element's position
            var elementPosition = _controller.ElementTransform.position;
            _sliderOffset = elementPosition - activatorPrimitive.GameObject.transform.position;
            SliderY.GameObject.transform.position = elementPosition;
            SliderY.Value = 0f;
            SliderY.Focus = (_controller.ElementTransform.position - _sliderOffset).ToVec();
            SliderY.LocalVisible = true;
        }
        
        /// <summary>
        /// Called when the z button has been activated.
        /// </summary>
        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            _startPosition = _controller.ElementTransform.position;

            CopyCurrentAssetTransform();
            _transformChangeConfirmed = false;
            
            Container.LocalVisible = false;

            var elementPosition = _controller.ElementTransform.position;
            _sliderOffset = elementPosition - activatorPrimitive.GameObject.transform.position;
            SliderZ.GameObject.transform.position = elementPosition;
            SliderZ.Value = 0f;
            SliderZ.Focus = (_controller.ElementTransform.position - _sliderOffset).ToVec();
            SliderZ.LocalVisible = true;
        }

        /// <summary>
        /// Called when SliderX slider is activated.
        /// </summary>
        private void SliderX_OnSliderValueConfirmed()
        {
            _transformChangeConfirmed = true;
            SliderX_OnUnfocused();
        }

        /// <summary>
        /// Called when SliderY slider is activated.
        /// </summary>
        private void SliderY_OnSliderValueConfirmed()
        {
            _transformChangeConfirmed = true;
            SliderY_OnUnfocused();
        }

        /// <summary>
        /// Called when SliderZ slider is activated.
        /// </summary>
        private void SliderZ_OnSliderValueConfirmed()
        {
            _transformChangeConfirmed = true;
            SliderZ_OnUnfocused();
        }

        /// <summary>
        /// Called when SliderScale slider is activated.
        /// </summary>
        private void SliderScale_OnSliderValueConfirmed()
        {
            _transformChangeConfirmed = true;
            SliderScale_OnUnfocused();
        }

        /// <summary>
        /// Called when SliderRotate slider is activated.
        /// </summary>
        private void SliderRotate_OnSliderValueConfirmed()
        {
            _transformChangeConfirmed = true;
            SliderRotate_OnUnfocused();
        }

    }
}
