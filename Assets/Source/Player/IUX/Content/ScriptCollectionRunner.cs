using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Runs a collection of scripts from a collection of script definitions.
    /// </summary>
    public class ScriptCollectionRunner
    {
        /// <summary>
        /// The state of script setup.
        /// </summary>
        public enum SetupState
        {
            /// <summary>
            /// Scripts aren't loaded.
            /// </summary>
            None,

            /// <summary>
            /// Scripts are initializing.
            /// </summary>
            Initializing,

            /// <summary>
            /// Scripts are ready for use.
            /// </summary>
            Done
        }

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
        /// Creates Script instances.
        /// </summary>
        private readonly IScriptFactory _scriptFactory;

        /// <summary>
        /// AppJsApi instance.
        /// </summary>
        private readonly AppJsApi _appJsApi;

        /// <summary>
        /// GameObject to attach scripts to.
        /// </summary>
        private readonly GameObject _root;

        /// <summary>
        /// The element running these scripts.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// Behaviors.
        /// </summary>
        private readonly List<BehaviorScript> _scriptComponents = new List<BehaviorScript>();

        /// <summary>
        /// Vines.
        /// </summary>
        private readonly List<VineScript> _vineComponents = new List<VineScript>();

        /// <summary>
        /// The current state of script loading.
        /// </summary>
        public SetupState CurrentState { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        [Construct]
        public ScriptCollectionRunner(
            IScriptManager scripts,
            IScriptRequireResolver resolver,
            IElementJsCache jsCache,
            IScriptFactory scriptFactory,
            AppJsApi appJsApi,
            GameObject root,
            Element element)
        {
            _scripts = scripts;
            _resolver = resolver;
            _jsCache = jsCache;
            _scriptFactory = scriptFactory;
            _appJsApi = appJsApi;
            _root = root;
            _element = element;
        }

        /// <summary>
        /// Constructor used for tests.
        /// </summary>
        public ScriptCollectionRunner(
            Widget widget, 
            IScriptFactory scriptFactory,
            IElementJsCache elementJsCache)
        {
            _root = widget.GameObject;
            _scriptFactory = scriptFactory;
            _jsCache = elementJsCache;
            _element = widget;
        }

        /// <summary>
        /// Starts up the scripts. Teardown must be called before this can be called again.
        /// </summary>
        /// <param name="scripts">The scripts to execute.</param>
        public void Setup(IList<EnkluScript> scripts)
        {
            if (CurrentState != SetupState.None)
            {
                throw new Exception("Already Setup!");
            }

            CurrentState = SetupState.Initializing;
            
            // start all vines first
            var len = scripts.Count;
            var vineSetupTokens = new List<IAsyncToken<Void>>(len);
            for (var i = 0; i < len; i++)
            {
                var script = scripts[i];
                if (null == script)
                {
                    continue;
                }

                if (script.Data.Type == ScriptType.Vine)
                {
                    vineSetupTokens.Add(RunVine(script));
                }
            }

            // start all behaviors after vines
            Async.All(vineSetupTokens.ToArray()).OnFinally(_ =>
            {
                // Check that scripts weren't unloaded while vines processed.
                if (CurrentState == SetupState.None)
                {
                    return;
                }

                for (var i = 0; i < len; i++) {
                    var script = scripts[i];
                    if (null == script) {
                        continue;
                    }

                    if (script.Data.Type == ScriptType.Behavior) {
                        RunBehavior(script);
                    }
                }

                CurrentState = SetupState.Done;
            });
        }

        /// <summary>
        /// Updates the scripts every frame.
        /// </summary>
        public void Update()
        {
            if (CurrentState != SetupState.Done)
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
            if (CurrentState == SetupState.None)
            {
                return;
            }

            CurrentState = SetupState.None;

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
        }

        /// <summary>
        /// Runs a vine script
        /// </summary>
        /// <param name="script">The vine to run.</param>
        private IAsyncToken<Void> RunVine(EnkluScript script)
        {
            Log.Info(this, "Run vine({0}) : {1}", script.Data, script.Source);

            var component = _scriptFactory.Vine(_element as Widget, script);
            _vineComponents.Add(component);
            
            return component
                .Configure()
                .OnSuccess(_ => component.Enter());
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
            host.SetValue("app", _appJsApi);
            host.SetValue("this", _jsCache.Element(_element));

            var component = _scriptFactory.Behavior(
                _element as Widget, _jsCache, host, script);
            _scriptComponents.Add(component);
            
            component.Configure();
            component.Enter();
        }
    }
}
