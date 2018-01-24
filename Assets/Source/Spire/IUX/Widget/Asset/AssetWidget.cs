/*using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Widget that loads + holds an asset.
    /// </summary>
    public class AssetWidget : Widget
    {
        /// <summary>
        /// Unique tag for this piece of <c>Content</c>.
        /// </summary>
        private readonly string _scriptTag = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Loads and executes scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Assembles asset.
        /// </summary>
        private readonly IAssetAssembler _assembler;
        
        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<AssetWidget> _onAssetLoaded = new MutableAsyncToken<AssetWidget>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly MutableAsyncToken<AssetWidget> _onScriptsLoaded = new MutableAsyncToken<AssetWidget>();

        /// <summary>
        /// Scripts.
        /// </summary>
        private readonly List<MonoBehaviourSpireScript> _scriptComponents = new List<MonoBehaviourSpireScript>();
        
        /// <summary>
        /// Scripting host.
        /// </summary>
        private readonly UnityScriptingHost _host;

        /// <summary>
        /// Token, lazily created through property OnLoaded.
        /// </summary>
        private IMutableAsyncToken<AssetWidget> _onLoaded;
        
        /// <summary>
        /// A token that is fired whenever the content has loaded.
        /// </summary>
        public IMutableAsyncToken<AssetWidget> OnLoaded
        {
            get
            {
                if (null == _onLoaded)
                {
                    _onLoaded = Async.Map(
                        Async.All(_onScriptsLoaded, _onAssetLoaded),
                        _ => this);
                }

                return _onLoaded;
            }
        }

        /// <summary>
        /// Called to setup the content.
        /// </summary>
        public AssetWidget(
            IScriptManager scripts,
            IAssetAssembler assembler,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages)
            : base(
                new GameObject("Asset"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _scripts = scripts;
            _assembler = assembler;
            _assembler.OnAssemblyComplete += Assembler_OnAssemblyComplete;

            _host = new UnityScriptingHost(
                this,
                null,
                _scripts);
        }

        /// <inheritdoc cref="Widget"/>
        protected override void AfterUnloadChildrenInternal()
        {
            _assembler.Teardown();
            TeardownScripts();
            
            base.AfterUnloadChildrenInternal();
        }

        /// <inheritdoc cref="Widget"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // assets
            _assembler.Setup(Schema);

            // scripts
            // TODO: watch for changes
            RefreshScripts();
        }
        
        /// <summary>
        /// Tears down scripts and sets them back up.
        /// </summary>
        private void RefreshScripts()
        {
            Log.Info(this, "Refresh scripts for {0}.", Schema);

            TeardownScripts();
            SetupScripts();
        }

        /// <summary>
        /// Loads all scripts.
        /// </summary>
        private void SetupScripts()
        {
            // TODO: pull from schema
            var scripts = new List<ScriptReference>();//Data.Scripts;
            var len = scripts.Count;

            Log.Info(this, "\t-Loading {0} scripts.", len);

            if (0 == len)
            {
                _onScriptsLoaded.Succeed(this);
                return;
            }

            var tokens = new IMutableAsyncToken<SpireScript>[len];
            for (var i = 0; i < len; i++)
            {
                var data = scripts[i];
                var script = _scripts.Create(data.ScriptDataId, _scriptTag);
                if (null == script)
                {
                    var error = string.Format(
                        "Could not create script from id {0}.",
                        data.ScriptDataId);

                    Log.Error(this, error);

                    tokens[i] = new MutableAsyncToken<SpireScript>(new Exception(
                        error));
                    continue;
                }

                var token = tokens[i] = script.OnReady;
                token
                    .OnSuccess(spireScript =>
                    {
                        // restart or create new component
                        MonoBehaviourSpireScript component = null;

                        var found = false;
                        for (int j = 0, jlen = _scriptComponents.Count; j < jlen; j++)
                        {
                            component = _scriptComponents[j];
                            if (component.Script == spireScript)
                            {
                                component.Exit();

                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            component = GameObject.AddComponent<MonoBehaviourSpireScript>();
                            _scriptComponents.Add(component);
                        }

                        component.Initialize(_host, script);
                        component.Enter();
                    });
            }

            // when all scripts are loaded, resolve the mutable token
            Async
                .All(tokens)
                .OnSuccess(_ => _onScriptsLoaded.Succeed(this))
                .OnFailure(_onScriptsLoaded.Fail);
        }

        /// <summary>
        /// Stop all the scripts.
        /// </summary>
        private void TeardownScripts()
        {
            Log.Info(this, "\t-Destroying {0} scripts.", _scriptComponents.Count);

            // destroy components
            for (int i = 0, len = _scriptComponents.Count; i < len; i++)
            {
                var component = _scriptComponents[i];
                component.Exit();
                UnityEngine.Object.Destroy(component);
            }
            _scriptComponents.Clear();

            // release scripts we created
            _scripts.ReleaseAll(_scriptTag);
        }
        
        /// <summary>
        /// Called when the assembler has completed seting up the asset.
        /// </summary>
        private void Assembler_OnAssemblyComplete(GameObject instance)
        {
            // parent + orient
            instance.transform.SetParent(GameObject.transform, false);
            instance.SetActive(true);

            _onAssetLoaded.Succeed(this);
        }
    }
}*/