using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for placing objects.
    /// </summary>
    [InjectVine("Design.PlaceItem")]
    public class PlaceObjectController : InjectableIUXController
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private PropController _controller;
        
        /// <summary>
        /// Elements.
        /// </summary>
        public FloatWidget Container
        {
            get { return (FloatWidget) Root; }
        }

        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; private set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; private set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; private set; }

        [InjectElements("..(@type==ContentWidget)")]
        public ContentWidget Content { get; private set; }

        [InjectElements("..content-container")]
        public ContainerWidget ContentContainer { get; private set; }

        /// <summary>
        /// Called to cancel placement.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Called to confirm placement.
        /// </summary>
        public event Action<PropData> OnConfirm;

        /// <summary>
        /// Called to confirm placement.
        /// </summary>
        public event Action<PropController> OnConfirmController;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnOk.Activator.OnActivated += Ok_OnActivated;
            BtnCancel.Activator.OnActivated += Cancel_OnActivated;
        }

        /// <summary>
        /// Initializes the menu with an existing prop.
        /// </summary>
        /// <param name="controller">The prop.</param>
        public void Initialize(PropController controller)
        {
            _controller = controller;
            _controller.HideSplashMenu();

            var controllerTransform = controller.Content.GameObject.transform;
            
            Content.LocalVisible = false;
            Container.GameObject.transform.position = controllerTransform.position;
            
            controllerTransform.SetParent(
                ContentContainer.GameObject.transform,
                true);

            BtnCancel.Schema.Set("visible", false);
        }

        /// <summary>
        /// Initializes the controller with a specific asset.
        /// </summary>
        /// <param name="assetId">The asset.</param>
        public void Initialize(string assetId)
        {
            _controller = null;

            Content.LocalVisible = true;
            Content.Schema.Set("assetSrc", assetId);

            BtnCancel.Schema.Set("visible", true);
        }
        
        /// <summary>
        /// Called when ok has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Ok_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != _controller)
            {
                if (null != OnConfirmController)
                {
                    OnConfirmController(_controller);
                }

                return;
            }

            var prop = PropData.Create(Content);
            if (null == prop)
            {
                Log.Error(this,
                    "Could not create PropData from ContentWidget {0}.", Content);
                if (null != OnCancel)
                {
                    OnCancel();
                }

                return;
            }

            if (null != OnConfirm)
            {
                OnConfirm(prop);
            }
        }

        /// <summary>
        /// Called when cancel has been activatd.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Cancel_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}