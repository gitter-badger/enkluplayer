using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        private readonly IElementUpdateDelegate _elementUpdater;

        /// <summary>
        /// Manages app scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Bridge.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Http.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Imports meshes.
        /// </summary>
        private readonly MeshImporter _importer;

        /// <summary>
        /// Main camera.
        /// </summary>
        private readonly Camera _mainCamera;

        /// <summary>
        /// Runtime gizmo system.
        /// </summary>
        private GameObject _runtimeGizmos;

        /// <summary>
        /// GameObject for mesh capture.
        /// </summary>
        private GameObject _meshCaptureGameObject;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _meshCaptureUrlProp;
        private ElementSchemaProp<float> _ambientIntensityProp;
        private ElementSchemaProp<string> _ambientColorProp;
        private ElementSchemaProp<bool> _ambientEnabledProp;
        private IAsyncToken<HttpResponse<byte[]>> _meshDownload;
        private DebugRendererMonoBehaviour _debugRenderer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DesktopDesignController(
            IElementUpdateDelegate elementUpdater,
            IAppSceneManager scenes,
            IBridge bridge,
            IHttpService http,
            MeshImporter importer,
            MainCamera mainCamera)
        {
            _elementUpdater = elementUpdater;
            _scenes = scenes;
            _bridge = bridge;
            _http = http;
            _importer = importer;
            _mainCamera = mainCamera.GetComponent<Camera>();
        }

        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            _runtimeGizmos = Object.Instantiate(context.PlayConfig.RuntimeGizmoSystem);

            // setup cameras + gizmos
            {
                var selectionSettings = _runtimeGizmos.GetComponentInChildren<EditorObjectSelection>().ObjectSelectionSettings;
                selectionSettings.CanSelectSpriteObjects = false;
                selectionSettings.CanSelectEmptyObjects = true;

                _runtimeGizmos.GetComponentInChildren<SceneGizmo>().Corner = SceneGizmoCorner.BottomRight;
                
                var camera = _runtimeGizmos.GetComponentInChildren<Camera>();
                camera.transform.LookAt(Vector3.zero);

                // move debug renderer
                var prevDebugRenderer = _mainCamera.GetComponent<DebugRendererMonoBehaviour>();
                _debugRenderer = camera.gameObject.AddComponent<DebugRendererMonoBehaviour>();
                _debugRenderer.Enabled = prevDebugRenderer.Enabled;
                _debugRenderer.Filter = prevDebugRenderer.Filter;
                Object.Destroy(prevDebugRenderer);
                Render.Renderer = _debugRenderer.Renderer;

                _mainCamera.enabled = false;
            }

            // setup updates
            {
                var scenes = app.Scenes.All;
                for (var i = 0; i < scenes.Length; i++)
                {
                    var id = scenes[i];
                    var root = app.Scenes.Root(id);
                    root.OnChildAdded += Root_OnChildAdded;

                    RecursivelyAddUpdater(root);
                }
            }

            //  setup property watching
            {
                var sceneId = app.Scenes.All[0];
                var sceneRoot = app.Scenes.Root(sceneId);
                
                _meshCaptureUrlProp = sceneRoot.Schema.Get<string>("meshcapture.relUrl");
                _meshCaptureUrlProp.OnChanged += MeshCapture_OnChanged;
                UpdateMeshCapture();

                _ambientEnabledProp = sceneRoot.Schema.Get<bool>("ambient.enabled");
                _ambientEnabledProp.OnChanged += AmbientEnabled_OnChanged;
                _ambientColorProp = sceneRoot.Schema.Get<string>("ambient.color");
                _ambientColorProp.OnChanged += AmbientColor_OnChanged;
                _ambientIntensityProp = sceneRoot.Schema.Get<float>("ambient.intensity");
                _ambientIntensityProp.OnChanged += AmbientIntensity_OnChanged;
                UpdateAmbientLighting();
            }

            EditorObjectSelection.Instance.SelectionChanged += Editor_OnSelectionChanged;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            if (null != _meshDownload)
            {
                _meshDownload.Abort();
                _meshDownload = null;
            }

            if (_meshCaptureGameObject)
            {
                Object.Destroy(_meshCaptureGameObject);
                _meshCaptureGameObject = null;
            }

            EditorObjectSelection.Instance.SelectionChanged -= Editor_OnSelectionChanged;

            _meshCaptureUrlProp.OnChanged -= MeshCapture_OnChanged;
            _ambientEnabledProp.OnChanged -= AmbientEnabled_OnChanged;
            _ambientColorProp.OnChanged -= AmbientColor_OnChanged;
            _ambientIntensityProp.OnChanged -= AmbientIntensity_OnChanged;

            _mainCamera.enabled = true;

            // move debug renderer
            var debugRenderer = _mainCamera.gameObject.AddComponent<DebugRendererMonoBehaviour>();
            debugRenderer.Enabled = _debugRenderer.Enabled;
            debugRenderer.Filter = _debugRenderer.Filter;
            Object.Destroy(_debugRenderer);
            Render.Renderer = debugRenderer.Renderer;

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
            var scene = _scenes.Root(sceneId);
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

            Log.Info(this, "Selecting {0}.", unityElement.GameObject);
            
            EditorObjectSelection.Instance.ClearSelection(false);
            EditorObjectSelection.Instance.SetSelectedObjects(
                new List<GameObject>{ unityElement.GameObject },
                false);
        }
        
        /// <summary>
        /// Recursively adds watchers to element and all children, recursively.
        /// </summary>
        /// <param name="root">The root.</param>
        private void RecursivelyAddUpdater(Element root)
        {
            AddUpdater(root);

            var children = root.Children;
            for (var index = 0; index < children.Count; index++)
            {
                AddUpdater(children[index]);
            }
        }

        /// <summary>
        /// Adds an updater to an element.
        /// </summary>
        /// <param name="element">The element to add updater to.</param>
        private void AddUpdater(Element element)
        {
            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                unityElement.GameObject
                    .AddComponent<ElementUpdateMonobehaviour>()
                    .Initialize(element, _elementUpdater);
            }
        }

        /// <summary>
        /// Updates ambient lighting settings.
        /// </summary>
        private void UpdateAmbientLighting()
        {
            var enabled = _ambientEnabledProp.Value;
            var hex = _ambientColorProp.Value;
            var intensity = _ambientIntensityProp.Value;

            Color color;
            if (!ColorFromHex(hex, out color))
            {
                Log.Warning(this, "Invalid ambient color '{0}' could't be parsed.", hex);
            }

            RenderSettings.ambientLight = enabled
                ? color
                : Color.black;
            RenderSettings.ambientIntensity = intensity;
        }

        /// <summary>
        /// Updates mesh capture settings.
        /// </summary>
        private void UpdateMeshCapture()
        {
            if (null != _meshDownload)
            {
                _meshDownload.Abort();
            }

            if (string.IsNullOrEmpty(_meshCaptureUrlProp.Value))
            {
                Log.Info(this, "No mesh capture to download.");
                return;
            }

            var url = _http.Urls.Url("meshcapture://" + _meshCaptureUrlProp.Value);

            Log.Info(this, "Downloading mesh capture from {0}...", url);

            // download
            _meshDownload = _http
                .Download(url)
                .OnSuccess(response =>
                {
                    Log.Info(this, "Mesh capture download complete. Starting import.");

                    if (null != _meshCaptureGameObject)
                    {
                        Object.Destroy(_meshCaptureGameObject);
                    }

                    _meshCaptureGameObject = new GameObject("MeshCapture");

                    // import
                    _importer.Import(response.Payload, (exception, action) =>
                    {
                        if (null != exception)
                        {
                            Log.Error(this, "Could not import mesh : {0}", exception);
                            return;
                        }

                        if (null == _meshCaptureGameObject)
                        {
                            return;
                        }

                        Log.Info(this, "Import complete. Constructing mesh.");

                        action(_meshCaptureGameObject);
                    });
                })
                .OnFailure(exception => Log.Error(this, "Could not download mesh capture : {0}", exception));
        }
        
        /// <summary>
        /// Called when a child is added.
        /// </summary>
        /// <param name="root">The element on which the event was called.</param>
        /// <param name="element">The element that was added.</param>
        private void Root_OnChildAdded(Element root, Element element)
        {
            RecursivelyAddUpdater(element);
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
                if (null == selected)
                {
                    return;
                }

                _bridge.Send(string.Format(
                    @"{{""type"":{0}, ""sceneId"":""{1}"", ""elementId"":""{2}""}}",
                    MessageTypes.BRIDGE_HELPER_SELECT,
                    _scenes.All[0],
                    selected.Element.Id));
            }
        }

        /// <summary>
        /// Called when ambient intensity changes.
        /// </summary>
        private void AmbientIntensity_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateAmbientLighting();
        }

        /// <summary>
        /// Called when ambient color changes.
        /// </summary>
        private void AmbientColor_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateAmbientLighting();
        }

        /// <summary>
        /// Called when ambient enabled changes.
        /// </summary>
        private void AmbientEnabled_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateAmbientLighting();
        }

        /// <summary>
        /// Called when mesh capture has changed.
        /// </summary>
        private void MeshCapture_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateMeshCapture();
        }

        /// <summary>
        /// Converts a color from hex to Color representation
        /// </summary>
        /// <param name="hex">The hex value.</param>
        /// <param name="color">The output Color.</param>
        /// <returns>True iff the hex value was valid.</returns>
        private static bool ColorFromHex(string hex, out Color color)
        {
            hex = (hex ?? "").Trim('#');
            if (hex.Length != 6)
            {
                color = Color.black;
                return false;
            }

            int r, g, b;
            try
            {
                r = Convert.ToInt32(hex.Substring(0, 2), 16);
                g = Convert.ToInt32(hex.Substring(2, 2), 16);
                b = Convert.ToInt32(hex.Substring(4, 2), 16);
            }
            catch
            {
                color = Color.black;
                return false;
            }

            color = new Color(r / 256f, g / 256f, b / 256f);
            return true;
        }
    }
}