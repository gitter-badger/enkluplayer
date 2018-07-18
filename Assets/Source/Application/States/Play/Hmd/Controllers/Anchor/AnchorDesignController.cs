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
            /// Caches world anchor data.
            /// </summary>
            public IWorldAnchorCache Cache;
            
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
        /// True iff show splash is requested.
        /// </summary>
        private bool _isSplashRequested;

        /// <summary>
        /// True iff controller is currently being edited.
        /// </summary>
        private bool _isEditing;

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
            
            SetupMarker();
            SetupSplash();
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();
            
            TeardownSplash();

            Renderer.gameObject.SetActive(false);
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
        /*
        /// <summary>
        /// Closes splash.
        /// </summary>
        public void CloseSplash()
        {
            _isSplashRequested = false;

            UpdateSplash();
        }

        /// <summary>
        /// Opens splash.
        /// </summary>
        public void OpenSplash()
        {
            _isSplashRequested = true;

            UpdateSplash();
        }
        */
        /// <summary>
        /// Updates the splash menu visibility.
        /// </summary>
        private void UpdateSplash()
        {
            _splash.enabled = !_isLocked && _isSplashRequested && !_isEditing;
        }
        
        /// <summary>
        /// Sets up the marker GameObject.
        /// </summary>
        private void SetupMarker()
        {
            if (null == Renderer)
            {
                Renderer = Instantiate(_config.AnchorPrefab, transform);
                Renderer.transform.localPosition = Vector3.zero;
                Renderer.transform.localRotation = Quaternion.identity;
                Renderer.Anchor = Anchor;
            }

            Renderer.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets up the splash menu.
        /// </summary>
        private void SetupSplash()
        {
            _splash = gameObject.GetComponent<ElementSplashController>();
            if (null == _splash)
            {
                _splash = gameObject.AddComponent<ElementSplashController>();
            }

            _isSplashRequested = true;
            _splash.OnOpen += Splash_OnOpen;

            UpdateSplash();
        }

        /// <summary>
        /// Tears down the splash menu.
        /// </summary>
        private void TeardownSplash()
        {
            _splash.OnOpen -= Splash_OnOpen;
            _splash.enabled = false;
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