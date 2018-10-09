using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Runs a collection of scripts from a collection of script definitions.
    /// </summary>
    public class ScriptCollectionRunner
    {
        /// <summary>
        /// Manages scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Resolves requires in scripts.
        /// </summary>
        private readonly IScriptRequireResolver _resolver;

        /// <summary>
        /// Js cache.
        /// </summary>
        private readonly IElementJsCache _jsCache;

        /// <summary>
        /// Creates ElementJS implementations.
        /// </summary>
        private readonly IElementJsFactory _elementJsFactory;

        /// <summary>
        /// GameObject to attach scripts to.
        /// </summary>
        private readonly GameObject _root;

        /// <summary>
        /// The element running these scripts.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// True iff Setup has been called but Teardown has not.
        /// </summary>
        private bool _isSetup;

        /// <summary>
        /// Tracks js caches in use.
        /// </summary>
        private readonly List<IElementJsCache> _caches = new List<IElementJsCache>();

        /// <summary>
        /// Behaviors.
        /// </summary>
        private readonly List<EnkluScriptElementBehavior> _scriptComponents = new List<EnkluScriptElementBehavior>();

        /// <summary>
        /// Vines.
        /// </summary>
        private readonly List<VineMonoBehaviour> _vineComponents = new List<VineMonoBehaviour>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptCollectionRunner(
            IScriptManager scripts,
            IScriptRequireResolver resolver,
            IElementJsCache jsCache,
            IElementJsFactory elementJsFactory,
            GameObject root,
            Element element)
        {
            _scripts = scripts;
            _resolver = resolver;
            _jsCache = jsCache;
            _elementJsFactory = elementJsFactory;
            _root = root;
            _element = element;
        }

        /// <summary>
        /// Starts up the scripts. Teardown must be called before this can be called again.
        /// </summary>
        /// <param name="scripts">The scripts to execute.</param>
        public void Setup(IList<EnkluScript> scripts)
        {
            if (_isSetup)
            {
                throw new Exception("Already Setup!");
            }

            _isSetup = true;
            
            // start all vines first
            var len = scripts.Count;
            for (var i = 0; i < len; i++)
            {
                var script = scripts[i];
                if (null == script)
                {
                    continue;
                }

                if (script.Data.Type == ScriptType.Vine)
                {
                    RunVine(script);
                }
            }

            // start all behaviors after vines
            for (var i = 0; i < len; i++)
            {
                var script = scripts[i];
                if (null == script)
                {
                    continue;
                }

                if (script.Data.Type == ScriptType.Behavior)
                {
                    RunBehavior(script);
                }
            }
        }

        /// <summary>
        /// Updates the scripts every frame.
        /// </summary>
        public void Update()
        {
            if (!_isSetup)
            {
                return;
            }

            for (int i = 0, len = _scriptComponents.Count; i < len; i++)
            {
                _scriptComponents[i].FrameUpdate();
            }
        }

        /// <summary>
        /// Stops executing scripts. This may be called multiple times without issue.
        /// </summary>
        public void Teardown()
        {
            if (!_isSetup)
            {
                return;
            }

            _isSetup = false;

            Log.Info(this, "\t-Destroying {0} scripts.", _scriptComponents.Count);

            // exit and destroy components
            for (int i = 0, len = _vineComponents.Count; i < len; i++)
            {
                var component = _vineComponents[i];
                component.Exit();

                UnityEngine.Object.Destroy(component);
            }
            _vineComponents.Clear();

            for (int i = 0, len = _scriptComponents.Count; i < len; i++)
            {
                var component = _scriptComponents[i];
                component.Exit();

                UnityEngine.Object.Destroy(component);
            }
            _scriptComponents.Clear();

            // clear caches
            for (int i = 0, len = _caches.Count; i < len; i++)
            {
                _caches[i].Clear();
            }
            _caches.Clear();
        }

        /// <summary>
        /// Runs a vine script
        /// </summary>
        /// <param name="script">The vine to run.</param>
        private void RunVine(EnkluScript script)
        {
            Log.Info(this, "Run vine({0}) : {1}", script.Data, script.Source);

            var component = GetVineComponent();
            component.Initialize(_element, script);
            component.Configure();
            component.Enter();
        }
        
        /// <summary>
        /// Runs a behavior script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        private void RunBehavior(EnkluScript script)
        {
            Log.Info(this, "RunBehavior({0}) : {1}", script.Data, script.Source);

            var host = new UnityScriptingHost(
                this,
                _resolver,
                _scripts);
            host.SetValue("system", SystemJsApi.Instance);
            host.SetValue("app", Main.NewAppJsApi(_jsCache));
            host.SetValue("this", _jsCache.Element(_element));
            _caches.Add(_jsCache);

            var component = GetBehaviorComponent();
            component.Initialize(_jsCache, _elementJsFactory, host, script, _element);
            component.Configure();
            component.Enter();
        }

        /// <summary>
        /// Retrieves a VineMonoBehaviour or creates one.
        /// </summary>
        /// <returns></returns>
        private VineMonoBehaviour GetVineComponent()
        {
            var component = _root.AddComponent<VineMonoBehaviour>();
            _vineComponents.Add(component);

            return component;
        }

        /// <summary>
        /// Retrieves a EnkluScriptElementBehavior or creates one.
        /// </summary>
        /// <returns></returns>
        private EnkluScriptElementBehavior GetBehaviorComponent()
        {
            var component = _root.AddComponent<EnkluScriptElementBehavior>();
            _scriptComponents.Add(component);

            return component;
        }
    }
}