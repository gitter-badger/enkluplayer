using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design controller for anchor widgets.
    /// </summary>
    public class AnchorDesignController : ElementDesignController
    {
        /// <summary>
        /// The context passed in.
        /// </summary>
        public class AnchorDesignControllerContext
        {
            /// <summary>
            /// Id of the app.
            /// </summary>
            public string AppId;

            /// <summary>
            /// Retrieves the scene id for an element.
            /// </summary>
            public Func<Element, string> SceneId;

            /// <summary>
            /// Configuration for playmode.
            /// </summary>
            public PlayModeConfig Config;
            
            /// <summary>
            /// Provides world anchor import/export.
            /// </summary>
            public IWorldAnchorProvider Provider;

            /// <summary>
            /// Http service.
            /// </summary>
            public IHttpService Http;

            /// <summary>
            /// Called to open adjust menu.
            /// </summary>
            public Action<AnchorDesignController> OnAdjust;
        }
        
        /// <summary>
        /// Configuration for play mode.
        /// </summary>
        private PlayModeConfig _config;
        
        /// <summary>
        /// Context.
        /// </summary>
        private AnchorDesignControllerContext _context;

        /// <summary>
        /// Splash menu.
        /// </summary>
        private ElementSplashController _splash;

        /// <summary>
        /// True iff locked.
        /// </summary>
        private bool _isLocked;
        
        /// <summary>
        /// The anchor widget.
        /// </summary>
        public WorldAnchorWidget Anchor
        {
            get { return (WorldAnchorWidget) Element; }
        }

        /// <summary>
        /// Renders an object.
        /// </summary>
        public AnchorRenderer Renderer { get; private set; }

        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);
            
            _context = (AnchorDesignControllerContext) context;
            _config = _context.Config;

            _splash = gameObject.AddComponent<ElementSplashController>();
            _splash.Root.Schema.Set("visible", false);
            _splash.OnOpen += Splash_OnOpen;
            _splash.Initialize(element);

            SetupMarker();
            UpdateSplash();
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();
            
            _splash.OnOpen -= Splash_OnOpen;
            Destroy(_splash);

            // Do NOT destroy the Renderer-- this is used to visualize the
            // anchor across all states.
        }
        
        /// <summary>
        /// Locks controller so it cannot be moved.
        /// </summary>
        public void Lock()
        {
            _isLocked = true;

            UpdateSplash();
        }

        /// <summary>
        /// Unlocks controller so it can be user again.
        /// </summary>
        public void Unlock()
        {
            _isLocked = false;

            UpdateSplash();
        }
        
        /// <summary>
        /// Updates the splash menu visibility.
        /// </summary>
        private void UpdateSplash()
        {
            _splash.Root.Schema.Set("visible", !_isLocked);
        }
        
        /// <summary>
        /// Sets up the marker GameObject.
        /// </summary>
        private void SetupMarker()
        {
            if (null == Renderer)
            {
                var isPrimary = Anchor.Schema.Get<string>(PrimaryAnchorManager.PROP_TAG_KEY).Value == PrimaryAnchorManager.PROP_TAG_VALUE;
                Renderer = Instantiate(
                    isPrimary ? _config.PrimaryAnchorPrefab : _config.AnchorPrefab,
                    transform);
                Renderer.transform.localPosition = Vector3.zero;
                Renderer.transform.localRotation = Quaternion.identity;
                Renderer.Anchor = Anchor;
            }

            Renderer.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Called when the splash menu open button is activated.
        /// </summary>
        private void Splash_OnOpen()
        {
            _context.OnAdjust(this);
        }
    }
}