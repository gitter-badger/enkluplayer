﻿using System;
using CreateAR.SpirePlayer.Assets;
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

        /// <summary>
        /// Loads assets.
        /// </summary>
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
        
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnOk.Activator.OnActivated += Ok_OnActivated;
            BtnCancel.Activator.OnActivated += Cancel_OnActivated;
        }

        /// <summary>
        /// Initializes the controller with a specific asset.
        /// </summary>
        /// <param name="assetId">The asset.</param>
        public void Initialize(string assetId)
        {
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
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}