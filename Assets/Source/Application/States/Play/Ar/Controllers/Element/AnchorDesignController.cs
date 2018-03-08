using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AnchorMovingState : IState
    {
        private readonly AnchorDesignController _controller;

        public AnchorMovingState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        public void Enter(object context)
        {
            _controller.Color = Color.green;
        }

        public void Update(float dt)
        {
            if (IsDirty() && CanExport())
            {
                _controller.ChangeState<AnchorSavingState>();
            }
        }

        private bool IsDirty()
        {
            return false;
        }

        private bool CanExport()
        {
            return true;
        }

        public void Exit()
        {
            
        }
    }

    public class AnchorSavingState : IState
    {
        private readonly AnchorDesignController _controller;
        private readonly IWorldAnchorProvider _provider;
        private readonly IHttpService _http;

        private IAsyncToken<byte[]> _exportToken;
        private IAsyncToken<HttpResponse<Trellis.Messages.UploadAnchor.Response>> _uploadToken;

        public AnchorSavingState(
            AnchorDesignController controller,
            IWorldAnchorProvider provider,
            IHttpService http)
        {
            _controller = controller;
            _provider = provider;
            _http = http;
        }

        public void Enter(object context)
        {
            _controller.Color = Color.yellow;
            _controller.CloseSplash();

            Export();
        }

        public void Update(float dt)
        {

        }

        public void Exit()
        {
            if (null != _exportToken)
            {
                _exportToken.Abort();
                _exportToken = null;
            }

            if (null != _uploadToken)
            {
                _uploadToken.Abort();
                _uploadToken = null;
            }
        }
        
        private void Export()
        {
            // first, export anchor
            _exportToken = _provider
                .Export(_controller.gameObject)
                .OnSuccess(bytes =>
                {
                    // next, upload anchor
                    _uploadToken = _http
                        .PostFile<Trellis.Messages.UploadAnchor.Response>(
                            _http.UrlBuilder.Url(string.Format(
                                "/v1/editor/app/{0}/scene/{1}/anchor/{2}",
                                "appId",
                                "sceneId",
                                _controller.Element.Id)),
                            new Commons.Unity.DataStructures.Tuple<string, string>[0],
                            ref bytes)
                        .OnSuccess(response =>
                        {
                            if (response.Payload.Success)
                            {
                                Log.Info(this, "Successfully uploaded world anchor.");

                                _controller.ChangeState<AnchorReadyState>();
                            }
                            else
                            {
                                Log.Error(this, "Could not upload world anchor : {0}.", response.Payload.Error);
                            }
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this,
                                "Could not upload world anchor : {0}.",
                                exception);
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not export anchor for {0} : {1}.",
                        _controller,
                        exception);
                });
        }
    }

    public class AnchorLoadingState : IState
    {
        private readonly AnchorDesignController _controller;
        private readonly IWorldAnchorProvider _provider;

        public AnchorLoadingState(
            AnchorDesignController controller,
            IWorldAnchorProvider provider)
        {
            _controller = controller;
            _provider = provider;
        }

        public void Enter(object context)
        {
            _controller.Color = Color.grey;
            _controller.CloseSplash();

            ((WorldAnchorWidget) _controller.Element)
                .Import()
                .OnSuccess(_ => _controller.ChangeState<AnchorReadyState>())
                .OnFailure(_ => _controller.ChangeState<AnchorReadyState>());
        }

        public void Update(float dt)
        {

        }

        public void Exit()
        {
            
        }
    }

    public class AnchorReadyState : IState
    {
        private readonly AnchorDesignController _controller;

        public AnchorReadyState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        public void Enter(object context)
        {
            _controller.Color = Color.white;
        }

        public void Update(float dt)
        {

        }

        public void Exit()
        {

        }
    }

    public class AnchorErrorState : IState
    {
        private readonly AnchorDesignController _controller;

        public AnchorErrorState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        public void Enter(object context)
        {
            _controller.Color = Color.red;
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            
        }
    }

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

        private FiniteStateMachine _fsm;

        private PlayModeConfig _config;
        private IWorldAnchorProvider _provider;
        private IHttpService _http;

        private AnchorDesignControllerContext _context;

        private AnchorSplashController _splash;

        private GameObject _marker;

        private Material[] _materials;

        private Color _color;

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

            _fsm = new FiniteStateMachine(new IState[]
            {
                new AnchorLoadingState(this, _provider),
                new AnchorReadyState(this),
                new AnchorMovingState(this),
                new AnchorSavingState(this, _provider, _http),
                new AnchorErrorState(this)
            });
            _fsm.Change<AnchorLoadingState>();
        }

        public override void Uninitialize()
        {
            base.Uninitialize();

            TeardownSplash();

            _marker.SetActive(false);
        }

        public void ChangeState<T>() where T : IState
        {
            _fsm.Change<T>();
        }

        public void FinalizeState()
        {

        }

        public void CloseSplash()
        {
            _splash.enabled = false;
        }

        public void ShowSplash()
        {
            _splash.enabled = true;
        }
        
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

        private void SetupMaterials()
        {
            var materials = new List<Material>();
            var renderers = _marker.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                materials.AddRange(renderers[i].sharedMaterials);
            }

            _materials = materials.ToArray();

            Color = Color.white;
        }

        private void SetupSplash()
        {
            _splash = gameObject.GetComponent<AnchorSplashController>();
            if (null == _splash)
            {
                _splash = gameObject.AddComponent<AnchorSplashController>();
            }

            _splash.enabled = true;
            _splash.OnOpen += Splash_OnOpen;
        }

        private void TeardownSplash()
        {
            _splash.OnOpen -= Splash_OnOpen;
            _splash.enabled = false;
        }

        private void Splash_OnOpen()
        {
            _context.OnAdjust(this);
        }
    }
}