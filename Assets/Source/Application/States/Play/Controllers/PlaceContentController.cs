using System;
using CreateAR.SpirePlayer.IUX;

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
            _controller.HideSplashMenu();

            var controllerTransform = controller.transform;
            
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
            
            var prop = new ElementData
            {
                Id = Guid.NewGuid().ToString(),
                Type = ElementTypes.CONTENT,
                Schema = new ElementSchemaData
                {
                    Strings =
                    {
                        { "assetSrc", Content.Data.Asset.AssetDataId },
                        { "name", Content.Data.Name }
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