using System;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the splash menu for anchors.
    /// </summary>
    [InjectVine("Element.Splash")]
    public class ElementSplashController : InjectableIUXController
    {
        /// <summary>
        /// The element!
        /// </summary>
        private Element _element;

        /// <summary>
        /// Assets.
        /// </summary>
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Splash button.
        /// </summary>
        public ButtonWidget BtnSplash
        {
            get { return (ButtonWidget) Root; }
        }

        /// <summary>
        /// Called when open requested.
        /// </summary>
        public event Action OnOpen;

        /// <summary>
        /// Initiailizes the controller.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Initialize(Element element)
        {
            _element = element;

            // If this controller is placed on a disabled GameObject, Awake won't
            // have been called.
            Inject();

            var isRoot = element.Id == "root";
            BtnSplash.Schema.Set(
                "label",
                isRoot ? "Root" : element.Schema.Get<string>("name").Value);

            if (!isRoot)
            {
                BtnSplash.Activator.OnActivated += Activator_OnActivated;
            }

            // choose a good local position
            UpdateMenuPosition();
        }
        
        /// <summary>
        /// Updates the menus position to something meaningful.
        /// </summary>
        private void UpdateMenuPosition()
        {
            var assetWidget = _element as ContentWidget;
            if (null != assetWidget)
            {
                var assetId = assetWidget.Schema.Get<string>("assetSrc").Value;
                var asset = Assets.Manifest.Asset(assetId);
                if (null != asset)
                {
                    var bounds = asset.Data.Stats.Bounds;
                    BtnSplash.GameObject.transform.localPosition = new Vector3(
                        bounds.Min.x + (bounds.Max.x - bounds.Min.x) / 2f,
                        bounds.Min.y + (bounds.Max.y - bounds.Min.y) / 2f,
                        bounds.Max.z);
                }
            }
        }

        /// <summary>
        /// Called when the activator is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Activator_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnOpen)
            {
                OnOpen();
            }
        }
    }
}