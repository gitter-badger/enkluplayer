using CreateAR.Commons.Unity.Async;
using RTEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design for desktops.
    /// </summary>
    public class DesktopDesignController : IDesignController
    {
        /// <summary>
        /// Transactions.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Manages element controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;
        
        /// <summary>
        /// Main camera.
        /// </summary>
        private readonly Camera _mainCamera;

        /// <summary>
        /// Filters based on types.
        /// </summary>
        private readonly TypeElementControllerFilter _contentFilter = new TypeElementControllerFilter(typeof(ContentWidget));

        /// <summary>
        /// Runtime gizmo system.
        /// </summary>
        private GameObject _runtimeGizmos;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DesktopDesignController(
            IElementTxnManager txns,
            IElementControllerManager controllers,
            MainCamera mainCamera)
        {
            _txns = txns;
            _controllers = controllers;
            _mainCamera = mainCamera.GetComponent<Camera>();
        }

        /// <inheritdoc />
        public void Setup(PlayModeConfig config, IAppController app)
        {
            _runtimeGizmos = Object.Instantiate(config.RuntimeGizmoSystem);
            var selectionSettings = _runtimeGizmos.GetComponentInChildren<EditorObjectSelection>().ObjectSelectionSettings;
            selectionSettings.CanSelectSpriteObjects = false;
            selectionSettings.CanSelectEmptyObjects = true;

            _mainCamera.enabled = false;

            var camera = _runtimeGizmos.GetComponentInChildren<Camera>();
            camera.transform.LookAt(Vector3.zero);

            _controllers.Active = true;
            _controllers
                .Filter(_contentFilter)
                .Add<DesktopContentDesignController>(null);
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _controllers
                .Remove<DesktopContentDesignController>()
                .Unfilter(_contentFilter);
            _controllers.Active = false;

            _mainCamera.enabled = true;
            Object.Destroy(_runtimeGizmos);
        }

        /// <inheritdoc />
        public IAsyncToken<string> Create()
        {
            throw new System.NotImplementedException();
        }
    }
}