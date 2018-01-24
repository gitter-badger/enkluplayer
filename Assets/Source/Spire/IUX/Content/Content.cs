using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Widget that loads + holds an asset.
    /// </summary>
    public class Content : Widget
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
        /// Assembles <c>Content</c>.
        /// </summary>
        private readonly IContentAssembler _assembler;

        /// <summary>
        /// True iff Setup has been called.
        /// </summary>
        private bool _setup;
        
        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<Content> _onAssetLoaded = new MutableAsyncToken<Content>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly MutableAsyncToken<Content> _onScriptsLoaded = new MutableAsyncToken<Content>();

        /// <summary>
        /// Scripts.
        /// </summary>
        private readonly List<MonoBehaviourSpireScript> _scriptComponents = new List<MonoBehaviourSpireScript>();

        /// <summary>
        /// Scripting host.
        /// </summary>
        private UnityScriptingHost _host;
        
        /// <summary>
        /// Token, lazily created through property OnLoaded.
        /// </summary>
        private IMutableAsyncToken<Content> _onLoaded;

        /// <summary>
        /// Content data.
        /// </summary>
        public ContentData Data { get; private set; }
        
        /// <summary>
        /// A token that is fired whenever the content has loaded.
        /// </summary>
        public IMutableAsyncToken<Content> OnLoaded
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
        /// Constructor.
        /// </summary>
        public Content(
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IScriptManager scripts,
            IContentAssembler assembler)
            : base(
                new GameObject("Content"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _scripts = scripts;
            _assembler = assembler;
            _assembler.OnAssemblyComplete += Assembler_OnAssemblyComplete;
        }
        
        /// <summary>
        /// Called to setup the content.
        /// </summary>
        /// <param name="data">Data to setup with.</param>
        public void Setup(ContentData data)
        {
            if (_setup)
            {
                throw new Exception(string.Format(
                    "Already initialized content. Existing: {0}, Requestd: {1}.",
                    Data.Name,
                    data.Name));
            }
            _setup = true;
            
            _host = new UnityScriptingHost(
                this,
                null,
                _scripts);
            
            UpdateData(data);
        }
        
        /// <summary>
        /// Destroys this instance. Should not be called directly, but through
        /// <c>IContentManager</c> Release flow.
        /// </summary>
        protected override void AfterUnloadChildrenInternal()
        {
            _assembler.Teardown();
            TeardownScripts();
        }

        /// <summary>
        /// Updates <c>ContentData</c> for this instance.
        /// </summary>
        public void UpdateData(ContentData data)
        {
            Log.Info(this, "Data updated for content {0}.", data);

            var assetRefresh = null == Data
                || Data.Asset.AssetDataId != data.Asset.AssetDataId;
            var scriptRefresh = ScriptRefreshRequired(data);
            
            Data = data;

            if (assetRefresh)
            {
                RefreshAsset();
            }

            if (scriptRefresh)
            {
                RefreshScripts();
            }
        }

        /// <summary>
        /// Updates the underlying material data.
        /// </summary>
        /// <param name="data">Material data to update with.</param>
        public void UpdateMaterialData(MaterialData data)
        {
            _assembler.UpdateMaterialData(data);
        }
        
        /// <summary>
        /// Tears down the asset and sets it back up.
        /// </summary>
        private void RefreshAsset()
        {
            Log.Info(this, "Refresh asset for {0}.", Data);

            _assembler.Teardown();
            _assembler.Setup(Data);
        }

        /// <summary>
        /// Tears down scripts and sets them back up.
        /// </summary>
        private void RefreshScripts()
        {
            Log.Info(this, "Refresh scripts for {0}.", Data);

            TeardownScripts();
            SetupScripts();
        }
        
        /// <summary>
        /// Loads all scripts.
        /// </summary>
        private void SetupScripts()
        {
            var scripts = Data.Scripts;
            var len = scripts.Length;

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
        /// True iff scripts need to be torn down and setup.
        /// </summary>
        /// <param name="data">New data.</param>
        /// <returns></returns>
        private bool ScriptRefreshRequired(ContentData data)
        {
            if (null == Data)
            {
                return true;
            }

            var currentScripts = Data.Scripts;
            var newScripts = data.Scripts;
            if (currentScripts.Length != newScripts.Length)
            {
                return true;
            }

            for (int i = 0, len = currentScripts.Length; i < len; i++)
            {
                if (currentScripts[i].ScriptDataId != newScripts[i].ScriptDataId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when the assembler has completed seting up the asset.
        /// </summary>
        private void Assembler_OnAssemblyComplete(GameObject instance)
        {
            // parent + orient
            instance.name = Data.Asset.AssetDataId;
            instance.transform.SetParent(GameObject.transform, false);
            instance.SetActive(true);

            _onAssetLoaded.Succeed(this);
        }
    }
}