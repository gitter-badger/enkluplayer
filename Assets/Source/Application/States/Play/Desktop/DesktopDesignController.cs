using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using RTEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design for desktops.
    /// </summary>
    public class DesktopDesignController : IDesignController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elements;

        /// <summary>
        /// Manages element controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;

        /// <summary>
        /// Txns.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Bridge.
        /// </summary>
        private readonly IBridge _bridge;

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
            IElementUpdateDelegate elements,
            IElementControllerManager controllers,
            IElementTxnManager txns,
            IBridge bridge,
            MainCamera mainCamera)
        {
            _elements = elements;
            _controllers = controllers;
            _txns = txns;
            _bridge = bridge;
            _mainCamera = mainCamera.GetComponent<Camera>();
        }

        /// <inheritdoc />
        public void Setup(PlayModeConfig config, IAppController app)
        {
            _runtimeGizmos = Object.Instantiate(config.RuntimeGizmoSystem);

            var selectionSettings = _runtimeGizmos.GetComponentInChildren<EditorObjectSelection>().ObjectSelectionSettings;
            selectionSettings.CanSelectSpriteObjects = false;
            selectionSettings.CanSelectEmptyObjects = true;

            _runtimeGizmos.GetComponentInChildren<SceneGizmo>().Corner = SceneGizmoCorner.BottomRight;

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

        /// <inheritdoc />
        public void Select(string sceneId, string elementId)
        {
            // find scene
            var scene = _txns.Root(sceneId);
            if (null == scene)
            {
                Log.Error(this, "Could not find scene root to select : {0}.", sceneId);
                return;
            }

            var element = scene.FindOne<Element>(".." + elementId);
            if (null == element)
            {
                Log.Error(this,
                    "Could not find element to select : {0}.",
                    elementId);
                return;
            }

            var unityElement = element as IUnityElement;
            if (null == unityElement)
            {
                Log.Error(this,
                    "Selected element is not an IUnityElement : {0}.",
                    elementId);
                return;
            }

            EditorObjectSelection.Instance.ClearSelection(false);
            EditorObjectSelection.Instance.SetSelectedObjects(
                new List<GameObject>{ unityElement.GameObject },
                false);
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
            
            // send to the OTHER SIDE
            if (args.SelectedObjects.Count == 1)
            {
                var selected = args.SelectedObjects[0].GetComponent<DesktopContentDesignController>();

                _bridge.Send(string.Format(
                    @"{{""type"":{0}, ""sceneId"":""{1}"", ""elementId"":""{2}""}}",
                    MessageTypes.BRIDGE_HELPER_SELECT,
                    _txns.TrackedScenes[0],
                    selected.Element.Id));
            }
            else
            {
                _bridge.Send(string.Format(
                    @"{{""type"":{0}}}",
                    MessageTypes.BRIDGE_HELPER_SELECT));
            }
        }
    }
}