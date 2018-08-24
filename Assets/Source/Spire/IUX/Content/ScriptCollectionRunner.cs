using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
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
        /// Behaviors.
        /// </summary>
        private readonly List<SpireScriptElementBehavior> _scriptComponents = new List<SpireScriptElementBehavior>();

        /// <summary>
        /// Vines.
        /// </summary>
        private readonly List<VineMonoBehaviour> _vineComponents = new List<VineMonoBehaviour>();
        
        public ScriptCollectionRunner(
            IScriptManager scripts,
            IElementJsFactory elementJsFactory,
            GameObject root,
            Element element)
        {
            _scripts = scripts;
            _elementJsFactory = elementJsFactory;
            _root = root;
            _element = element;
        }

        /// <summary>
        /// Starts up the scripts. Teardown must be called before this can be called again.
        /// </summary>
        /// <param name="scripts">The scripts to execute.</param>
        public void Setup(IList<SpireScript> scripts)
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

            // exit components (we reuse these later)
            for (int i = 0, len = _vineComponents.Count; i < len; i++)
            {
                _vineComponents[i].Exit();
            }

            for (int i = 0, len = _scriptComponents.Count; i < len; i++)
            {
                _scriptComponents[i].Exit();
            }
        }

        /// <summary>
        /// Runs a vine script
        /// </summary>
        /// <param name="script">The vine to run.</param>
        private void RunVine(SpireScript script)
        {
            Log.Info(this, "Run vine({0}) : {1}", script.Data, script.Source);

            var component = GetVineComponent(script.Data.Id);
            component.Initialize(_element, script);
            component.Configure();
            component.Enter();
        }
        
        /// <summary>
        /// Runs a behavior script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        private void RunBehavior(SpireScript script)
        {
            Log.Info(this, "RunBehavior({0}) : {1}", script.Data, script.Source);

            var host = new UnityScriptingHost(
                this,
                null,
                _scripts);
            var jsCache = new ElementJsCache(_elementJsFactory, host);
            host.SetValue("app", Main.NewAppJsApi(jsCache));
            host.SetValue("this", jsCache.Element(_element));

            var component = GetBehaviorComponent(script.Data.Id);
            component.Initialize(_elementJsFactory, host, script, _element);
            component.Configure();
            component.Enter();
        }

        /// <summary>
        /// Retrieves a VineMonoBehaviour or creates one.
        /// </summary>
        /// <param name="id">The id of the script to get one for.</param>
        /// <returns></returns>
        private VineMonoBehaviour GetVineComponent(string id)
        {
            VineMonoBehaviour component;
            for (int j = 0, jlen = _vineComponents.Count; j < jlen; j++)
            {
                component = _vineComponents[j];
                if (component.Script.Data.Id == id)
                {
                    return component;
                }
            }

            component = _root.AddComponent<VineMonoBehaviour>();
            _vineComponents.Add(component);

            return component;
        }

        /// <summary>
        /// Retrieves a SpireScriptElementBehavior or creates one.
        /// </summary>
        /// <param name="id">The id of the script to get one for.</param>
        /// <returns></returns>
        private SpireScriptElementBehavior GetBehaviorComponent(string id)
        {
            SpireScriptElementBehavior component;
            for (int j = 0, jlen = _scriptComponents.Count; j < jlen; j++)
            {
                component = _scriptComponents[j];
                if (component.Script.Data.Id == id)
                {
                    return component;
                }
            }

            component = _root.AddComponent<SpireScriptElementBehavior>();
            _scriptComponents.Add(component);

            return component;
        }
    }
}