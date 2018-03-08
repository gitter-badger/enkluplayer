using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Naive implementation of IElementControllerManager. It's a tad tricky.
    /// </summary>
    public class ElementControllerManager : IElementControllerManager
    {
        /// <summary>
        /// Tracks relationship between type, context, and controllers.
        /// </summary>
        private class ControllerBinding
        {
            /// <summary>
            /// True iff this binding should be applied.
            /// </summary>
            public bool Active;

            /// <summary>
            /// The type of controller to add to GameObjects.
            /// </summary>
            public Type ControllerType;

            /// <summary>
            /// The context to pass into Initialize methods.
            /// </summary>
            public object Context;

            /// <summary>
            /// List of controllers.
            /// </summary>
            public readonly List<ElementDesignController> Controllers = new List<ElementDesignController>();
        }

        /// <summary>
        /// Manages the scene.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Scratch list used in a few methods.
        /// </summary>
        private readonly List<IUnityElement> _elementScratch = new List<IUnityElement>();
        
        /// <summary>
        /// List of filters to apply.
        /// </summary>
        private readonly List<IElementControllerFilter> _filters = new List<IElementControllerFilter>();

        /// <summary>
        /// List of elements that match filters.
        /// </summary>
        private readonly List<IUnityElement> _filteredElements = new List<IUnityElement>();

        /// <summary>
        /// All bindings. Bindings are not destroyed when removed, just deactivated.
        /// </summary>
        private readonly List<ControllerBinding> _bindings = new List<ControllerBinding>();

        /// <summary>
        /// Backing variable for Active property.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// True iff the manager should be adding/removing controllers.
        /// </summary>
        public bool Active
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;

                if (_isActive)
                {
                    _txns.OnSceneAfterTracked += Txns_OnSceneAfterTracked;
                    _txns.OnSceneBeforeUntracked += Txns_OnSceneBeforeUntracked;

                    // track all
                    for (int i = 0, len = _txns.TrackedScenes.Length; i < len; i++)
                    {
                        TrackScene(_txns.TrackedScenes[i]);
                    }
                }
                else
                {
                    _txns.OnSceneAfterTracked -= Txns_OnSceneAfterTracked;
                    _txns.OnSceneBeforeUntracked -= Txns_OnSceneBeforeUntracked;

                    // untrack all
                    for (int i = 0, len = _txns.TrackedScenes.Length; i < len; i++)
                    {
                        UntrackScene(_txns.TrackedScenes[i]);
                    }
                }

                Reapply();
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementControllerManager(IElementTxnManager txns)
        {
            _txns = txns;
        }

        /// <inheritdoc />
        public IElementControllerManager Filter(IElementControllerFilter filter)
        {
            _filters.Add(filter);

            // apply new filter to existing filtered items, i.e. a new filter
            // will only _remove_ items
            for (var i = _filteredElements.Count - 1; i >= 0; i--)
            {
                var element = _filteredElements[i];
                if (!filter.Include((Element) element))
                {
                    // this element is no longer part of the filtered set,
                    // remove controllers
                    RemoveControllers(element);

                    _filteredElements.RemoveAt(i);
                }
            }

            return this;
        }

        /// <inheritdoc />
        public IElementControllerManager Unfilter(IElementControllerFilter filter)
        {
            _filters.Remove(filter);
            
            Reapply();

            return this;
        }

        /// <inheritdoc />
        public IElementControllerManager Add<T>(object context) where T : ElementDesignController
        {
            var binding = Binding(typeof(T));
            if (null != binding && binding.Active)
            {
                return this;
            }

            if (null == binding)
            {
                binding = new ControllerBinding
                {
                    ControllerType = typeof(T),
                    Context = context
                };

                _bindings.Add(binding);
            }
            
            binding.Active = true;

            AddControllersToFilteredList(binding);

            return this;
        }

        /// <inheritdoc />
        public IElementControllerManager Remove<T>() where T : ElementDesignController
        {
            var binding = Binding(typeof(T));
            if (null != binding)
            {
                _bindings.Remove(binding);

                RemoveControllersFromFilteredList(binding);

                binding.Active = false;
            }

            return this;
        }

        /// <inheritdoc />
        public void All<T>(IList<T> collection) where T : ElementDesignController
        {
            var binding = Binding(typeof(T));
            if (null != binding)
            {
                for (var i = 0; i < binding.Controllers.Count; i++)
                {
                    collection.Add((T) binding.Controllers[i]);
                }
            }
        }

        /// <summary>
        /// Full reapply.
        /// </summary>
        private void Reapply()
        {
            // gather all unity elements in all scenes
            _elementScratch.Clear();
            var sceneIds = _txns.TrackedScenes;
            for (var i = 0; i < sceneIds.Length; i++)
            {
                AddUnityElements(_txns.Root(sceneIds[i]), _elementScratch);
            }

            // compare with current filtered list of elements
            for (int i = 0, len = _elementScratch.Count; i < len; i++)
            {
                var element = _elementScratch[i];

                // if the element is already part of the filtered list, we can
                // safely ignore
                var found = false;
                for (int j = 0, jlen = _filteredElements.Count; j < jlen; j++)
                {
                    if (_filteredElements[j] == element)
                    {
                        found = true;
                        break;
                    }
                }

                // otherwise, this element needs to be added to the filtered list
                if (!found)
                {
                    AddAllControllersToElement(element);

                    _filteredElements.Add(element);
                }
            }
        }

        /// <summary>
        /// Retrieves a binding for a controller type.
        /// </summary>
        /// <param name="controllerType">The type of controller that should be added.</param>
        /// <returns></returns>
        private ControllerBinding Binding(Type controllerType)
        {
            for (var i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                if (binding.ControllerType == controllerType)
                {
                    return binding;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds unity elements in a hierarchy to a list.
        /// </summary>
        /// <param name="element">The element to start with.</param>
        /// <param name="accumulator">The list to add to.</param>
        private void AddUnityElements(Element element, List<IUnityElement> accumulator)
        {
            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                accumulator.Add(unityElement);
            }

            var children = element.Children;
            for (var i = 0; i < children.Count; i++)
            {
                AddUnityElements(children[i], accumulator);
            }
        }

        /// <summary>
        /// True iff all filters include this element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private bool Include(Element element)
        {
            for (var i = 0; i < _filters.Count; i++)
            {
                var filter = _filters[i];
                if (!filter.Include(element))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes all controllers from an element.
        /// </summary>
        /// <param name="element">The element.</param>
        private void RemoveControllers(IUnityElement element)
        {
            for (var i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                var controller = (ElementDesignController) element.GameObject.GetComponent(binding.ControllerType);
                controller.OnDestroyed -= Controller_OnDestroyed;
                controller.Uninitialize();

                binding.Controllers.Remove(controller);
            }
        }

        /// <summary>
        /// Removes controllers for a specific binding from all elements in the
        /// filtered list.
        /// </summary>
        /// <param name="binding">The binding.</param>
        private void RemoveControllersFromFilteredList(ControllerBinding binding)
        {
            var controllers = binding.Controllers;
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].Uninitialize();
            }

            controllers.Clear();
        }

        /// <summary>
        /// Adds controller from a specific binding to all elements in the
        /// filtered list.
        /// </summary>
        /// <param name="binding">The binding.</param>
        private void AddControllersToFilteredList(ControllerBinding binding)
        {
            for (var i = 0; i < _filteredElements.Count; i++)
            {
                binding.Controllers.Add(AddController(
                    binding.ControllerType,
                    binding.Context,
                    _filteredElements[i]));
            }
        }

        /// <summary>
        /// Adds a controller type from a binding to an element.
        /// </summary>
        /// <param name="controllerType">The type of controller to add.</param>
        /// <param name="context">The context to pass the controller.</param>
        /// <param name="unityElement">The element to add to.</param>
        /// <returns></returns>
        private ElementDesignController AddController(
            Type controllerType,
            object context,
            IUnityElement unityElement)
        {
            var gameObject = unityElement.GameObject;

            var controller = (ElementDesignController) gameObject.GetComponent(controllerType);
            if (null == controller)
            {
                controller = (ElementDesignController) gameObject.AddComponent(controllerType);
            }

            controller.OnDestroyed += Controller_OnDestroyed;
            controller.Initialize((Element) unityElement, context);

            return controller;
        }

        /// <summary>
        /// Adds controllers from all bindings to an element.
        /// </summary>
        /// <param name="unityElement">The element.</param>
        private void AddAllControllersToElement(IUnityElement unityElement)
        {
            for (int i = 0, len = _bindings.Count; i < len; i++)
            {
                var binding = _bindings[i];
                if (binding.Active)
                {
                    binding.Controllers.Add(AddController(
                        binding.ControllerType,
                        binding.Context,
                        unityElement));
                }
            }
        }

        /// <summary>
        /// Called when an element is removed.
        /// </summary>
        /// <param name="root">The root element.</param>
        private void ElementRemoved(Element root)
        {
            // gather all unity elements
            _elementScratch.Clear();
            AddUnityElements(root, _elementScratch);

            // remove all controllers
            for (var i = _elementScratch.Count - 1; i >= 0; i--)
            {
                var element = _elementScratch[i];

                if (_filteredElements.Remove(element))
                {
                    RemoveControllers(element);
                }
            }
        }

        /// <summary>
        /// Called when an element has been added.
        /// </summary>
        /// <param name="root">The root element.</param>
        private void ElementAdded(Element root)
        {
            // gather all unity elements
            _elementScratch.Clear();
            AddUnityElements(root, _elementScratch);

            // filter elements
            for (var i = _elementScratch.Count - 1; i >= 0; i--)
            {
                var element = _elementScratch[i];
                if (Include((Element)element))
                {
                    _filteredElements.Add(element);

                    AddAllControllersToElement(element);
                }
            }
        }

        /// <summary>
        /// Tracks a scene.
        /// </summary>
        /// <param name="sceneId">Id of the scene.</param>
        private void TrackScene(string sceneId)
        {
            var root = _txns.Root(sceneId);

            ElementAdded(root);

            // listen for future child updates
            root.OnChildAdded += SceneRoot_OnChildAdded;
            root.OnChildRemoved += SceneRoot_OnChildRemoved;
        }

        /// <summary>
        /// Untracks a scene.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        private void UntrackScene(string sceneId)
        {
            var root = _txns.Root(sceneId);

            // stop listening to root
            root.OnChildAdded -= SceneRoot_OnChildAdded;
            root.OnChildRemoved -= SceneRoot_OnChildRemoved;

            // remove controllers
            ElementRemoved(root);
        }

        /// <summary>
        /// Called before a scene becomes untracked.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        private void Txns_OnSceneBeforeUntracked(string sceneId)
        {
            UntrackScene(sceneId);
        }

        /// <summary>
        /// Called after a scene is tracked.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        private void Txns_OnSceneAfterTracked(string sceneId)
        {
            TrackScene(sceneId);
        }

        /// <summary>
        /// Called after a child is added to a scene.
        /// </summary>
        /// <param name="root">The root element.</param>
        /// <param name="element">The child that was added.</param>
        private void SceneRoot_OnChildAdded(Element root, Element element)
        {
            ElementAdded(element);
        }

        /// <summary>
        /// Called after a child is removed from a scene.
        /// </summary>
        /// <param name="root">The root element.</param>
        /// <param name="element">The child that was removed.</param>
        private void SceneRoot_OnChildRemoved(Element root, Element element)
        {
            // the controllers have already been removed via Controller_OnDestroyed flow
            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                _filteredElements.Remove(unityElement);
            }
        }

        /// <summary>
        /// Called when a controller is about to be destroyed.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void Controller_OnDestroyed(ElementDesignController controller)
        {
            // finding binding + remove it
            var type = controller.GetType();
            for (var i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                if (binding.ControllerType == type)
                {
                    if (binding.Controllers.Remove(controller))
                    {
                        controller.Uninitialize();
                    }

                    break;
                }
            }
        }
    }
}