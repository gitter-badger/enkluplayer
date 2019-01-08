using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using RLD;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
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
        /// Primary anchor.
        /// </summary>
        private readonly IPrimaryAnchorManager _primaryAnchor;
        
        /// <summary>
        /// Contains editor settings.
        /// </summary>
        private readonly EditorSettings _editorSettings;

        /// <summary>
        /// Runtime gizmo system.
        /// </summary>
        private GameObject _runtimeGizmos;

        /// <summary>
        /// Origin Reference Gameobject.
        /// </summary>
        private GameObject _referenceCube;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _ambientIntensityProp;
        private ElementSchemaProp<string> _ambientColorProp;
        private ElementSchemaProp<bool> _ambientEnabledProp;
        private DesktopControlBarView _controlBar;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DesktopDesignController(
            IElementUpdateDelegate elementUpdater,
            IAppSceneManager scenes,
            IBridge bridge,
            IPrimaryAnchorManager primaryAnchor,
            EditorSettings editorSettings)
        {
            _elementUpdater = elementUpdater;
            _scenes = scenes;
            _bridge = bridge;
            _primaryAnchor = primaryAnchor;
            _editorSettings = editorSettings;
        }

        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            _runtimeGizmos = Object.Instantiate(context.PlayConfig.RuntimeGizmoSystem);

            // settings
            var focus = _runtimeGizmos.GetComponentInChildren<RTFocusCamera>();
            focus.Hotkeys.Pan.IsEnabled = true;
            focus.Hotkeys.LookAround.IsEnabled = true;
            focus.Hotkeys.Orbit.IsEnabled = true;
            focus.ZoomSettings.PerspStandardZoomSensitivity = 100;

            var selection = _runtimeGizmos.GetComponentInChildren<RTObjectSelectionGizmos>();
            selection.Hotkeys.ActivateMoveGizmo.IsEnabled = true;
            selection.Hotkeys.ActivateMoveGizmo.Key = KeyCode.Q;
            selection.Hotkeys.ActivateRotationGizmo.IsEnabled = true;
            selection.Hotkeys.ActivateRotationGizmo.Key = KeyCode.W;
            selection.Hotkeys.ActivateScaleGizmo.IsEnabled = true;
            selection.Hotkeys.ActivateScaleGizmo.Key = KeyCode.E;
            selection.Hotkeys.ToggleTransformSpace.IsEnabled = true;
            selection.Hotkeys.ToggleTransformSpace.Key = KeyCode.R;
            selection.SetTransformSpace(GizmoSpace.Local);

            _controlBar = Object.Instantiate(context.PlayConfig.ControlBar);
            _controlBar.transform.SetParent(GameObject.Find("IUX").transform, false);

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

            // primary anchor setup
            _primaryAnchor.Setup();

            // setup property watching
            {
                var sceneId = app.Scenes.All[0];
                var sceneRoot = app.Scenes.Root(sceneId);

                _ambientEnabledProp = sceneRoot.Schema.Get<bool>("ambient.enabled");
                _ambientEnabledProp.OnChanged += AmbientEnabled_OnChanged;
                _ambientColorProp = sceneRoot.Schema.Get<string>("ambient.color");
                _ambientColorProp.OnChanged += AmbientColor_OnChanged;
                _ambientIntensityProp = sceneRoot.Schema.Get<float>("ambient.intensity");
                _ambientIntensityProp.OnChanged += AmbientIntensity_OnChanged;
                UpdateAmbientLighting();
            }

            // initialize reference object
            SetupReferenceObject();

            RTObjectSelection.Get.Changed += Editor_OnSelectionChanged;
            
            _editorSettings.OnChanged += Editor_OnSettingsChanged;
            _editorSettings.Update();
        }

        /// <inheritdoc />
        public void Teardown()
        {
            RTObjectSelection.Get.Changed -= Editor_OnSelectionChanged;

            _ambientEnabledProp.OnChanged -= AmbientEnabled_OnChanged;
            _ambientColorProp.OnChanged -= AmbientColor_OnChanged;
            _ambientIntensityProp.OnChanged -= AmbientIntensity_OnChanged;
            
            Object.Destroy(_controlBar);
            Object.Destroy(_runtimeGizmos);
            Object.Destroy(_referenceCube);
            
            _editorSettings.OnChanged -= Editor_OnSettingsChanged;
        }

        /// <inheritdoc />
        public IAsyncToken<string> Create()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Select(string sceneId, string elementId)
        {
            // process deselect
            if (string.IsNullOrEmpty(sceneId) && string.IsNullOrEmpty(elementId))
            {
                RTObjectSelection.Get.ClearSelection(true);
                return;
            }

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
            
            var updater = unityElement.GameObject.GetComponent<ElementUpdateMonobehaviour>();
            if (null == updater)
            {
                Log.Error(this,
                    "Selected element is in a bad state and does not have ElementUpdateMonoBehaviour : {0}",
                    elementId);
                return;
            }

            // do nothing if this cannot be selected anyway
            if (!updater.OnCanBeSelected(null))
            {
                return;
            }

            RTObjectSelection.Get.ClearSelection(true);
            RTObjectSelection.Get.SetSelectedObjects(
                new List<GameObject> { unityElement.GameObject },
                true);
        }

        /// <inheritdoc />
        public void Focus(string sceneId, string elementId)
        {
            // find scene
            var scene = _scenes.Root(sceneId);
            if (null == scene)
            {
                Log.Error(this, "Could not find scene root to focus on : {0}.", sceneId);
                return;
            }

            var element = scene.FindOne<Element>(".." + elementId);
            if (null == element)
            {
                Log.Error(this,
                    "Could not find element to focus on : {0}.",
                    elementId);
                return;
            }

            var unityElement = element as IUnityElement;
            if (null == unityElement)
            {
                Log.Error(this,
                    "Focused element is not an IUnityElement : {0}.",
                    elementId);
                return;
            }

            var updater = unityElement.GameObject.GetComponent<ElementUpdateMonobehaviour>();
            if (null == updater)
            {
                Log.Error(this,
                    "Focused element is in a bad state and does not have ElementUpdateMonoBehaviour : {0}",
                    elementId);
                return;
            }

            // do nothing if this cannot be selected anyway
            if (!updater.OnCanBeSelected(null))
            {
                return;
            }

            RTObjectSelection.Get.ClearSelection(true);
            RTObjectSelection.Get.SetSelectedObjects(
                new List<GameObject> { unityElement.GameObject },
                false);
            RTFocusCamera.Get.Focus(RTObjectSelection.Get.GetWorldAABB());
        }

        /// <summary>
        /// Setup up a reference object for user to determine origin
        /// </summary>
        private void SetupReferenceObject()
        {
            var bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            _referenceCube = new GameObject("ReferenceObject");
            _referenceCube.transform.position = new Vector3(0, 0, 0);
            _referenceCube.transform.rotation = Quaternion.identity;

            var outline = _referenceCube.gameObject.GetComponent<ModelLoadingOutline>();
            if (null == outline)
            {
                outline = _referenceCube.gameObject.AddComponent<ModelLoadingOutline>();
                _referenceCube.gameObject.AddComponent<ReferenceObjectAxesRenderer>();
            }

            outline.Init(bounds);

            //Sets the reference object created as child of primary anchor if found
            _primaryAnchor.OnPrimaryLocated(() =>
            {
                WorldAnchorWidget primaryAnchorWidget = _primaryAnchor.Anchor;
                if (primaryAnchorWidget != null)
                {
                    _referenceCube.transform.SetParent(primaryAnchorWidget.GameObject.transform, false);
                    Log.Info(this, "Reference cube added as child of primary anchor");
                }
            });

            _referenceCube.SetActive(_editorSettings.Grid);
        }

        /// <summary>
        /// Recursively adds watchers to element and all children, recursively.
        /// </summary>
        /// <param name="root">The root.</param>
        private void RecursivelyAddUpdater(Element root)
        {
            AddUpdaterComponent(root);

            var children = root.Children;
            for (var index = 0; index < children.Count; index++)
            {
                RecursivelyAddUpdater(children[index]);
            }
        }

        /// <summary>
        /// Adds an updater to an element.
        /// </summary>
        /// <param name="element">The element to add updater to.</param>
        private void AddUpdaterComponent(Element element)
        {
            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                if (IsIUX(element))
                {
                    unityElement.GameObject.AddComponent<NonSelectableMonoBehaviour>();
                }
                else
                {
                    unityElement.GameObject
                        .AddComponent<ElementUpdateMonobehaviour>()
                        .Initialize(element, _elementUpdater);
                }
            }
        }

        /// <summary>
        /// True iff the element is an IUX element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private bool IsIUX(Element element)
        {
            return element is ButtonWidget;
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
            // send to the OTHER SIDE
            var selectedObjs = args.ObjectsWhichWereSelected;
            if (selectedObjs.Count == 1)
            {
                var selected = selectedObjs[0].GetComponent<ElementUpdateMonobehaviour>();
                if (null == selected)
                {
                    Log.Error(this, "No controller.");
                    return;
                }

                _bridge.Send(string.Format(
                    @"{{""type"":{0}, ""sceneId"":""{1}"", ""elementId"":""{2}""}}",
                    MessageTypes.BRIDGE_HELPER_SELECT,
                    _scenes.All[0],
                    selected.Element.Id));
            }
            else if (0 == selectedObjs.Count)
            {
                _bridge.Send(string.Format(
                    @"{{""type"":{0}}}",
                    MessageTypes.BRIDGE_HELPER_SELECT));
            }
        }

        /// <summary>
        /// Called when the a setting has changed in the editor.
        /// </summary>
        /// <param name="args">The event args from EditorSettings.</param>
        private void Editor_OnSettingsChanged(SettingChangedArgs args)
        {
            if (args.Type == EditorSettingsTypes.Grid || args.Type == EditorSettingsTypes.All)
            {
                if (null != _referenceCube)
                {
                    _referenceCube.SetActive(_editorSettings.Grid);                    
                }
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