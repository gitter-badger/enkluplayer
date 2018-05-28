using System;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the menu for placing objects.
    /// </summary>
    [InjectVine("Design.PlaceContent")]
    public class PlaceContentController : InjectableIUXController
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private ContentDesignController _controller;

        /// <summary>
        /// Previous position.
        /// </summary>
        private Vector3 _previousPosition;

        /// <summary>
        /// Previous parent.
        /// </summary>
        private Transform _previousParent;
        
        /// <summary>
        /// Elements.
        /// </summary>
        public FloatWidget Container
        {
            get { return (FloatWidget) Root; }
        }

        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; set; }

        [InjectElements("..(@type==ContentWidget)")]
        public ContentWidget Content { get; set; }

        [InjectElements("..content-container")]
        public ContainerWidget ContentContainer { get; set; }

        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Called to cancel placement.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Called to confirm placement.
        /// </summary>
        public event Action<ElementData> OnConfirm;

        /// <summary>
        /// Called to confirm placement.
        /// </summary>
        public event Action<ContentDesignController> OnConfirmController;

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
        public void Initialize(ContentDesignController controller)
        {
            _controller = controller;
            _controller.DisableUpdates();
            _controller.HideSplashMenu();

            // save previous position
            _previousPosition = _controller.transform.position;
            _previousParent = _controller.transform.parent;

            // hide content we use for placement
            Content.LocalVisible = false;

            // parent to ContentContainer
            //Container.GameObject.transform.position = controller.transform.position;
            controller.transform.SetParent(
                ContentContainer.GameObject.transform,
                true);
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
                _controller.transform.SetParent(_previousParent, true);
                _controller.ShowSplashMenu();
                _controller.EnableUpdates();

                if (null != OnConfirmController)
                {
                    OnConfirmController(_controller);
                }

                return;
            }

            var assetId = Content.Schema.GetOwn("assetSrc", string.Empty).Value;
            var assetData = Assets.Manifest.Data(assetId);
            var element = new ElementData
            {
                Id = Guid.NewGuid().ToString(),
                Type = ElementTypes.CONTENT,
                Schema = new ElementSchemaData
                {
                    Strings =
                    {
                        { "assetSrc", assetId },
                        { "name", assetData.AssetName }
                    },
                    Vectors =
                    {
                        { "position", Content.GameObject.transform.position.ToVec() },
                        { "rotation", Content.GameObject.transform.rotation.eulerAngles.ToVec() },
                        { "scale", Content.GameObject.transform.localScale.ToVec() }
                    }
                }
            };
            
            if (null != OnConfirm)
            {
                OnConfirm(element);
            }
        }

        /// <summary>
        /// Called when cancel has been activatd.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Cancel_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != _controller)
            {
                _controller.transform.SetParent(_previousParent, true);
                _controller.transform.position = _previousPosition;
                _controller.ShowSplashMenu();
                _controller.EnableUpdates();
            }

            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}