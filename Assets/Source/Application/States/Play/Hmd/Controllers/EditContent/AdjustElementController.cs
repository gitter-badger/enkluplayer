﻿using System;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for adjusting a prop.
    /// </summary>
    [InjectVine("Element.Adjust")]
    public class AdjustElementController : InjectableIUXController
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
        public bool drawAxis { get; private set; }

        public Vector3 p0 = new Vector3(-10f, 0f, 2f);
        public Vector3 p1 = new Vector3(10f, 0f, 2f);
        private Material _lineMaterial;
        /// <summary>
        /// Called when the menu should be exited.
        /// </summary>
        public event Action<ElementSplashDesignController> OnExit;

        /// <summary>
        /// Loads assets.
        /// </summary>
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Initiailizes the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        public void Initialize(ElementSplashDesignController controller)
        {
            _controller = controller;

            var assetSrcId = _controller.Element.Schema.Get<string>("assetSrc").Value;
            var assetData = Assets.Manifest.Data(assetSrcId);
            var BoundsData = assetData.Stats.Bounds;

            Bounds Bounds = new Bounds();
            Bounds.max = new Vector3(BoundsData.Max.x, BoundsData.Max.y, BoundsData.Max.z);
            Bounds.min = new Vector3(BoundsData.Min.x, BoundsData.Min.y, BoundsData.Min.z);

            ModelLoadingOutline outline = _controller.gameObject.AddComponent<ModelLoadingOutline>();
            outline.Init(Bounds);

            ResetMenuPosition();
            enabled = true;
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnRotate.Activator.OnActivated += BtnRotate_OnActivated;
            BtnScale.Activator.OnActivated += BtnScale_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;

            SliderX.OnUnfocused += SliderX_OnUnfocused;
            SliderX.OnSliderValueConfirmed += SliderX_OnSliderValueConfirmed;

            SliderY.OnUnfocused += SliderY_OnUnfocused;
            SliderY.OnSliderValueConfirmed += SliderY_OnSliderValueConfirmed;

            SliderZ.OnUnfocused += SliderZ_OnUnfocused;
            SliderZ.OnSliderValueConfirmed += SliderZ_OnSliderValueConfirmed;

            SliderScale.OnUnfocused += SliderScale_OnUnfocused;
            SliderScale.OnSliderValueConfirmed += SliderScale_OnSliderValueConfirmed;

            SliderRotate.OnUnfocused += SliderRotate_OnUnfocused;
            SliderRotate.OnSliderValueConfirmed += SliderRotate_OnSliderValueConfirmed;

            _lineMaterial = new Material(Shader.Find("Unlit/LoadProgressIndicator"));

        }

        private void SliderX_OnSliderValueConfirmed()
        {
         
            SliderX_OnUnfocused();
        }
        private void SliderY_OnSliderValueConfirmed()
        {
       
            SliderY_OnUnfocused();
        }
        private void SliderZ_OnSliderValueConfirmed()
        {
         
            SliderZ_OnUnfocused();
        }
        private void SliderScale_OnSliderValueConfirmed()
        {
           
            SliderScale_OnUnfocused();
        }
        private void SliderRotate_OnSliderValueConfirmed()
        {
          
            SliderRotate_OnUnfocused();
        }

        // Will be called from camera after regular rendering is done.
        public void OnRenderObject()
        {
            if (drawAxis)
            {
                GL.PushMatrix();
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
                _lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);

                GL.Color(new Color(0f, 0f, 0f, 1f));
                GL.Vertex(p0);
                GL.Vertex(p1);
                GL.End();
                GL.PopMatrix();
            }
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

            if (SliderScale.Visible)
            {
                const float scaleDiff = 2f;
                var start = _startScale.x;
                var val = start + (SliderScale.Value - 0.5f) * scaleDiff;
                _controller.transform.localScale = val * Vector3.one;
               
            }

            if (SliderRotate.Visible)
            {
                var start = _startRotation.y;
                var val = start + (SliderRotate.Value - 0.5f) * 360;
                _controller.transform.localRotation = Quaternion.Euler(0, val, 0);
               
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

            _controller.FinalizeState();
            drawAxis = false;
        }

        /// <summary>
        /// Called when the y slider has unfocused.
        /// </summary>
        private void SliderY_OnUnfocused()
        {
            SliderY.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;

            _controller.FinalizeState();
            drawAxis = false;
        }

        /// <summary>
        /// Called when the z slider has unfocused.
        /// </summary>
        private void SliderZ_OnUnfocused()
        {
            SliderZ.LocalVisible = false;

            ResetMenuPosition();
            Container.LocalVisible = true;

            _controller.FinalizeState();
            drawAxis = false;
        }

        /// <summary>
        /// Called when the scale slider has unfocused.
        /// </summary>
        private void SliderScale_OnUnfocused()
        {
            SliderScale.LocalVisible = false;
            Container.LocalVisible = true;

            _controller.FinalizeState();
            drawAxis = false;
        }

        /// <summary>
        /// Called when the rotate slider has unfocused.
        /// </summary>
        private void SliderRotate_OnUnfocused()
        {
            SliderRotate.LocalVisible = false;
            Container.LocalVisible = true;

            _controller.FinalizeState();
            drawAxis = false;
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
            drawAxis = true;
            Container.LocalVisible = false;

            _startRotation = _controller.transform.localRotation.eulerAngles;
            SliderRotate.LocalVisible = true;

            setAxisCoordinates(SliderRotate);
        }

        /// <summary>
        /// Called when the scale button has been activated.
        /// </summary>
        private void BtnScale_OnActivated(ActivatorPrimitive activatorPrimitive) 
        {
            drawAxis = true;
            Container.LocalVisible = false;

            _startScale = _controller.transform.localScale;
            SliderScale.LocalVisible = true;

            setAxisCoordinates(SliderScale);
        }
        
        /// <summary>
        /// Called when the x button has been activated.
        /// </summary>
        private void BtnX_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            drawAxis = true;
            Container.LocalVisible = false;
            SliderX.LocalVisible = true;
            setAxisCoordinates(SliderX);

        }

        /// <summary>
        /// Called when the y button has been activated.
        /// </summary>
        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
            drawAxis = true;
            SliderY.LocalVisible = true;
            setAxisCoordinates(SliderY);
        }
        

        /// <summary>
        /// Called when the z button has been activated.
        /// </summary>
        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            drawAxis = true;
            _startPosition = _controller.transform.position;
            _startForward = Intention.Forward.ToVector();

            Container.LocalVisible = false;
            SliderZ.LocalVisible = true;

            setAxisCoordinates(SliderZ);
        }

        private void setAxisCoordinates(SliderWidget Sliderwidget)
        {
            Vector3[] points = Sliderwidget.getPivotPoints();
            if (points.Length > 0 && !points[0].Equals(Vector3.zero) && !points[1].Equals(Vector3.zero))
            {
                p0 = new Vector3(points[0].x, points[0].y, points[0].z);
                p1 = new Vector3(points[1].x, points[1].y, points[1].z);
            }
        }
    }
}