using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using RTEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
        /// Elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elements;

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
            IElementUpdateDelegate elements,
            IElementControllerManager controllers,
            MainCamera mainCamera)
        {
            _txns = txns;
            _elements = elements;
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
                .Add<DesktopContentDesignController>(new DesktopContentDesignController.DesktopContentDesignControllerContext
                {
                    Delegate = _elements
                });

            EditorObjectSelection.Instance.SelectionChanged += Editor_OnSelectionChanged;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            EditorObjectSelection.Instance.SelectionChanged -= Editor_OnSelectionChanged;

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the selection has changed.
        /// </summary>
        /// <param name="args">Event args.</param>
        private void Editor_OnSelectionChanged(ObjectSelectionChangedEventArgs args)
        {
            // disable updates
            for (var i = 0; i < args.DeselectedObjects.Count; i++)
            {
                var gameObject = args.DeselectedObjects[i];
                var controller = gameObject.GetComponent<DesktopContentDesignController>();
                if (null != controller)
                {
                    controller.DisableUpdates();
                }
            }

            // enable updates
            for (var i = 0; i < args.SelectedObjects.Count; i++)
            {
                var gameObject = args.SelectedObjects[i];
                var controller = gameObject.GetComponent<DesktopContentDesignController>();
                if (null != controller)
                {
                    controller.EnableUpdates();
                }
            }
        }
    }
}