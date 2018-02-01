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
    public class ContentWidget : Widget
    {
        /// <summary>
        /// Unique tag for this piece of <c>Content</c>.
        /// </summary>
        private readonly string _scriptTag = System.Guid.NewGuid().ToString();

        /// <summary>
        /// App data.
        /// </summary>
        private readonly IAppDataManager _appData;

        /// <summary>
        /// Loads and executes scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Assembles <c>Content</c>.
        /// </summary>
        private readonly IContentAssembler _assembler;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _srcProp;
        
        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onAssetLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onScriptsLoaded = new MutableAsyncToken<ContentWidget>();

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
        private IMutableAsyncToken<ContentWidget> _onLoaded;

        /// <summary>
        /// Content data.
        /// </summary>
        public ContentData Data { get; private set; }
        
        /// <summary>
        /// A token that is fired whenever the content has loaded.
        /// </summary>
        public IMutableAsyncToken<ContentWidget> OnLoaded
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
        public ContentWidget(
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IScriptManager scripts,
            IContentAssembler assembler,
            IAppDataManager appData)
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
            _appData = appData;
        }

        /// <summary>
        /// Updates <c>ContentData</c> for this instance.
        /// </summary>
        public void UpdateData(ContentData data)
        {
            Log.Info(this, "Data updated for content {0}.", data);

            var assetRefresh = null == Data || Data.Asset.AssetDataId != data.Asset.AssetDataId;
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

        /// <inheritdoc />
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _host = new UnityScriptingHost(
                this,
                null,
                _scripts);

            _srcProp = Schema.Get<string>("src");
            _srcProp.OnChanged += Src_OnChanged;

            UpdateData(_appData.Get<ContentData>(_srcProp.Value));
        }

        /// <inheritdoc />
        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            _srcProp.OnChanged -= Src_OnChanged;

            _assembler.Teardown();
            TeardownScripts();
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
            if (null == Data)
            {
                return;
            }

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

        /// <summary>
        /// Called when the source changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Src_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateData(_appData.Get<ContentData>(_srcProp.Value));
        }
    }
}