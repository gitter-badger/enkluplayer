using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
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
        /// State machine used internally.
        /// </summary>
        private FiniteStateMachine _fsm;

        /// <summary>
        /// Configuration for play mode.
        /// </summary>
        private PlayModeConfig _config;

        /// <summary>
        /// Provides anchoring import + export.
        /// </summary>
        private IWorldAnchorProvider _provider;

        /// <summary>
        /// Http service.
        /// </summary>
        private IHttpService _http;

        /// <summary>
        /// Context.
        /// </summary>
        private AnchorDesignControllerContext _context;

        /// <summary>
        /// Splash menu.
        /// </summary>
        private AnchorSplashController _splash;

        /// <summary>
        /// GameObject representation.
        /// </summary>
        private GameObject _marker;

        /// <summary>
        /// Materials.
        /// </summary>
        private Material[] _materials;

        /// <summary>
        /// True iff locked.
        /// </summary>
        private bool _isLocked;

        /// <summary>
        /// True iff show splash is requested.
        /// </summary>
        private bool _isSplashRequested;

        /// <summary>
        /// Backing variable for Color prop.
        /// </summary>
        private Color _color;

        /// <summary>
        /// Sets the marker color.
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;

                for (var i = 0; i < _materials.Length; i++)
                {
                    _materials[i].SetColor("_Color", _color);
                }
            }
        }

        /// <summary>
        /// The anchor widget.
        /// </summary>
        public WorldAnchorWidget Anchor
        {
            get { return (WorldAnchorWidget) Element; }
        }

        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (AnchorDesignControllerContext) context;

            _config = _context.Config;
            _provider = _context.Provider;
            _http = _context.Http;

            SetupMarker();
            SetupMaterials();
            SetupSplash();

            // initialize() -> load state (lock) -> ready state
            //                                   -> error state
            // beginEdit() -> move state -> finalizeEdit() -> save state (lock) -> ready state
            //                                                                  -> error state
            //                           -> cancel() -> load state (lock) -> ready state
            //                                                            -> error state

            _fsm = new FiniteStateMachine(new IState[]
            {
                new AnchorLoadingState(this),
                new AnchorSavingState(this, _provider, _http),
                new AnchorReadyState(this),
                new AnchorMovingState(this),
                new AnchorErrorState(this)
            });
            
            ChangeState<AnchorLoadingState>();
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();

            _fsm.Change(null);

            TeardownSplash();

            _marker.SetActive(false);
        }

        /// <summary>
        /// Changes anchor state.
        /// </summary>
        /// <typeparam name="T">The state to change to.</typeparam>
        public void ChangeState<T>() where T : IState
        {
            Log.Info(this, "Change state to {0}.", typeof(T).Name);

            _fsm.Change<T>();
        }

        /// <summary>
        /// Starts edit mode.
        /// </summary>
        public void BeginEdit()
        {
            ChangeState<AnchorMovingState>();
        }

        /// <summary>
        /// Finalizes changes, reuploads.
        /// </summary>
        public void FinalizeEdit()
        {
            ChangeState<AnchorSavingState>();
        }

        /// <summary>
        /// Aborts changes and reverts.
        /// </summary>
        public void AbortEdit()
        {
            ChangeState<AnchorLoadingState>();
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
        public void ShowSplash()
        {
            _isSplashRequested = true;

            UpdateSplash();
        }

        /// <summary>
        /// Updates the splash menu visibility.
        /// </summary>
        private void UpdateSplash()
        {
            Log.Info(this, "{0}: {1} - {2}", Element.Id, _isLocked, _isSplashRequested);

            _splash.enabled = !_isLocked && _isSplashRequested;
        }
        
        /// <summary>
        /// Sets up the marker GameObject.
        /// </summary>
        private void SetupMarker()
        {
            if (null == _marker)
            {
                _marker = Instantiate(_config.AnchorPrefab, transform);
                _marker.transform.localPosition = Vector3.zero;
                _marker.transform.localRotation = Quaternion.identity;
            }

            _marker.SetActive(true);
        }

        /// <summary>
        /// Sets up materials.
        /// </summary>
        private void SetupMaterials()
        {
            var materials = new List<Material>();
            var renderers = _marker.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                materials.AddRange(renderers[i].materials);
            }

            _materials = materials.ToArray();

            Color = Color.white;
        }

        /// <summary>
        /// Sets up the splash menu.
        /// </summary>
        private void SetupSplash()
        {
            _splash = gameObject.GetComponent<AnchorSplashController>();
            if (null == _splash)
            {
                _splash = gameObject.AddComponent<AnchorSplashController>();
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