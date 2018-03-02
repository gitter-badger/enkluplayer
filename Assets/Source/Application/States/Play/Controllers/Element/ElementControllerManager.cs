using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class ElementControllerManager : IElementControllerManager
    {
        private class ControllerBinding
        {
            public bool Active;
            public Type ControllerType;
            public object Context;

            public readonly List<ElementDesignController> Controllers = new List<ElementDesignController>();
        }

        private readonly IElementTxnManager _txns;
        private readonly List<IUnityElement> _elementScratch = new List<IUnityElement>();
        
        private readonly List<Element> _roots = new List<Element>();
        private readonly List<IElementControllerFilter> _filters = new List<IElementControllerFilter>();
        private readonly List<IUnityElement> _filteredElements = new List<IUnityElement>();

        private readonly List<ControllerBinding> _bindings = new List<ControllerBinding>();

        private bool _isActive;

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
                }
                else
                {
                    _txns.OnSceneAfterTracked -= Txns_OnSceneAfterTracked;
                    _txns.OnSceneBeforeUntracked -= Txns_OnSceneBeforeUntracked;
                }
            }
        }
        
        public ElementControllerManager(IElementTxnManager txns)
        {
            _txns = txns;
        }

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

        public IElementControllerManager Unfilter(IElementControllerFilter filter)
        {
            _filters.Remove(filter);
            
            // gather all unity elements in all scenes
            _elementScratch.Clear();
            for (var i = 0; i < _roots.Count; i++)
            {
                AddUnityElements(_roots[i], _elementScratch);
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
                    AddControllerToAllBindings(element);

                    _filteredElements.Add(element);
                }
            }

            return this;
        }
        
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

        private void AddUnityElements(Element element, List<IUnityElement> elements)
        {
            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                elements.Add(unityElement);
            }

            var children = element.Children;
            for (var i = 0; i < children.Count; i++)
            {
                AddUnityElements(children[i], elements);
            }
        }

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

        private void RemoveControllers(IUnityElement element)
        {
            for (var i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                var controller = (ElementDesignController)element.GameObject.GetComponent(binding.ControllerType);
                controller.Uninitialize();

                binding.Controllers.Remove(controller);
            }
        }

        private void RemoveControllersFromFilteredList(ControllerBinding binding)
        {
            var controllers = binding.Controllers;
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].Uninitialize();
            }

            controllers.Clear();
        }

        private void AddControllersToFilteredList(ControllerBinding binding)
        {
            for (var i = 0; i < _filteredElements.Count; i++)
            {
                binding.Controllers.Add(AddController(binding, _filteredElements[i]));
            }
        }
        
        private ElementDesignController AddController(
            ControllerBinding binding,
            IUnityElement unityElement)
        {
            var gameObject = unityElement.GameObject;

            var controller = (ElementDesignController) (
                gameObject.GetComponent(binding.ControllerType)
                ?? gameObject.AddComponent(binding.ControllerType));
            controller.Initialize((Element) unityElement, binding.Context);

            return controller;
        }

        private void AddControllerToAllBindings(IUnityElement unityElement)
        {
            for (int i = 0, len = _bindings.Count; i < len; i++)
            {
                var binding = _bindings[i];
                if (binding.Active)
                {
                    binding.Controllers.Add(AddController(binding, unityElement));
                }
            }
        }

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

                    AddControllerToAllBindings(element);
                }
            }
        }

        private void Txns_OnSceneBeforeUntracked(string sceneId)
        {
            var root = _txns.Root(sceneId);
            if (_roots.Remove(root))
            {
                // stop listening
                root.OnChildAdded -= SceneRoot_OnChildAdded;
                root.OnChildRemoved -= SceneRoot_OnChildRemoved;

                ElementRemoved(root);
            }
        }

        private void Txns_OnSceneAfterTracked(string sceneId)
        {
            var root = _txns.Root(sceneId);
            _roots.Add(root);

            ElementAdded(root);

            // listen for future child updates
            root.OnChildAdded += SceneRoot_OnChildAdded;
            root.OnChildRemoved += SceneRoot_OnChildRemoved;
        }
        
        private void SceneRoot_OnChildAdded(Element root, Element element)
        {
            ElementAdded(element);
        }

        private void SceneRoot_OnChildRemoved(Element root, Element element)
        {
            ElementRemoved(element);
        }
    }
}