using System;
using Boo.Lang;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;
using LightJson;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ScriptCollectionRunner
    {
        private readonly IScriptManager _scripts;
        private readonly IElementJsFactory _elementJsFactory;
        private readonly Engine _host;
        private readonly GameObject _root;
        private readonly Element _element;

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
            Engine host,
            GameObject root,
            Element element)
        {
            _scripts = scripts;
            _elementJsFactory = elementJsFactory;
            _host = host;
            _root = root;
            _element = element;
        }

        public void Setup(SpireScript[] scripts)
        {
            _isSetup = true;

            // start all vines first
            var len = scripts.Length;
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

        public void Teardown()
        {
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

            var component = GetBehaviorComponent(script.Data.Id);
            component.Initialize(_elementJsFactory, _host, script, _element);
            component.Configure();
            component.Enter();
        }

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

    /// <summary>
    /// Widget that loads + holds an asset, behaviors, and vines.
    /// </summary>
    public class ContentWidget : Widget
    {
        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onAssetLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onScriptsLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Loads and executes scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Assembles <c>Content</c>.
        /// </summary>
        private readonly IContentAssembler _assembler;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementJsFactory _elementJsFactory;

        /// <summary>
        /// Unique tag used for managing scripts.
        /// </summary>
        private string _scriptTag;

        /// <summary>
        /// Caches js objects.
        /// </summary>
        private IElementJsCache _jsCache;

        /// <summary>
        /// Runs scripts.
        /// </summary>
        private ScriptCollectionRunner _runner;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _srcAssetProp;
        private ElementSchemaProp<string> _scriptsProp;

        /// <summary>
        /// Scripting host.
        /// </summary>
        private UnityScriptingHost _host;
        
        /// <summary>
        /// Token, lazily created through property OnLoaded.
        /// </summary>
        private IMutableAsyncToken<ContentWidget> _onLoaded;

        /// <summary>
        /// Set to true when we should poll to update the asset.
        /// </summary>
        private bool _pollUpdateAsset;

        /// <summary>
        /// Set to true when we should poll to update scripts.
        /// </summary>
        private bool _pollRefreshScript;

        /// <summary>
        /// Token responsible for all script loads.
        /// </summary>
        private IAsyncToken<SpireScript[]> _loadScriptsToken;

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
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IScriptManager scripts,
            IContentAssembler assembler,
            IElementJsFactory elementFactory )
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _scripts = scripts;
            _assembler = assembler;
            _elementJsFactory = elementFactory;
            _assembler.OnAssemblyComplete += Assembler_OnAssemblyComplete;
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            _scriptTag = Id;
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _host = new UnityScriptingHost(
                this,
                null,
                _scripts);
            _jsCache = new ElementJsCache(_elementJsFactory, _host);
            _host.SetValue("app", Main.NewAppJsApi(_jsCache));
            _host.SetValue("this", _jsCache.Element(this));

            _srcAssetProp = Schema.GetOwn("assetSrc", "");
            _srcAssetProp.OnChanged += AssetSrc_OnChanged;

            _scriptsProp = Schema.GetOwn("scripts", "[]");
            _scriptsProp.OnChanged += Scripts_OnChanged;
            
            UpdateAsset();
            RefreshScripts();
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();
            
            _srcAssetProp.OnChanged -= AssetSrc_OnChanged;
            _scriptsProp.OnChanged -= Scripts_OnChanged;

            _assembler.Teardown();
            AbortScripts();
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_pollUpdateAsset)
            {
                UpdateAsset();
            }

            if (_pollRefreshScript)
            {
                RefreshScripts();
            }

            if (null != _runner)
            {
                _runner.Update();
            }
        }

        /// <summary>
        /// Determines if this piece of content should load its assets.
        /// </summary>
        /// <returns></returns>
        private bool ShouldLoadAsset()
        {
            if (!Visible)
            {
                return false;
            }

            if (DeviceHelper.IsHoloLens())
            {
                return PrimaryAnchorManager.AreAllAnchorsReady;
            }

            return true;
        }

        /// <summary>
        /// Tears down the asset and sets it back up.
        /// </summary>
        private void UpdateAsset()
        {
            _pollUpdateAsset = false;

            if (!ShouldLoadAsset())
            {
                _pollUpdateAsset = true;
                return;
            }

            LogVerbose("Refresh asset for {0}.", Id);

            _assembler.Teardown();
            _assembler.Setup(
                GameObject.transform,
                _srcAssetProp.Value);
        }

        /// <summary>
        /// Tears down scripts and sets them back up.
        /// </summary>
        private void RefreshScripts()
        {
            _pollRefreshScript = false;

            if (!ShouldLoadAsset())
            {
                _pollRefreshScript = true;
                return;
            }

            Log.Info(this, "Refresh scripts for {0}.", Id);

            if (null == _runner)
            {
                _runner = new ScriptCollectionRunner(
                    _scripts,
                    _elementJsFactory,
                    _host,
                    GameObject,
                    this);
            }

            AbortScripts();

            // TODO: reset element -- all props need reset from data

            _loadScriptsToken = LoadScripts();
            _loadScriptsToken
                .OnSuccess(scripts =>
                {
                    _onScriptsLoaded.Succeed(this);

                    _runner.Setup(scripts);
                })
                .OnFailure(ex =>
                {
                    Log.Warning(this, "Could not load scripts : {0}", ex);

                    _onScriptsLoaded.Fail(ex);
                });
        }

        private void AbortScripts()
        {
            // abort load handler
            if (null != _loadScriptsToken)
            {
                _loadScriptsToken.Abort();
                _loadScriptsToken = null;
            }

            // stop running scripts
            _runner.Teardown();

            // release scripts we created
            _scripts.ReleaseAll(_scriptTag);
        }

        private string[] GetScriptIds()
        {
            var scriptsSrc = _scriptsProp.Value;

            // unescape-- this is dumb obviously
            scriptsSrc = scriptsSrc.Replace("\\\"", "\"");

            JsonArray value;
            try
            {
                value = JsonValue.Parse(scriptsSrc).AsJsonArray;
            }
            catch (Exception exception)
            {
                Log.Info(this, "Could not parse \"{0}\" : {1}.",
                    scriptsSrc,
                    exception);

                _onScriptsLoaded.Succeed(this);
                return new string[0];
            }

            var len = value.Count;
            var ids = new string[len];
            for (var i = 0; i < len; i++)
            {
                ids[i] = value[i]["id"].AsString;
            }

            return ids;
        }

        /// <summary>
        /// Loads all scripts.
        /// </summary>
        private IAsyncToken<SpireScript[]> LoadScripts()
        {
            var ids = GetScriptIds();
            var len = ids.Length;

            Log.Info(this, "\tLoading {0} scripts.", len);

            if (0 == len)
            {
                return new AsyncToken<SpireScript[]>(new SpireScript[0]);
            }

            var scripts = new SpireScript[len];
            var tokens = new IMutableAsyncToken<SpireScript>[len];
            for (var i = 0; i < len; i++)
            {
                var scriptId = ids[i];
                var script = scripts[i] = _scripts.Create(scriptId, _scriptTag);
                if (null == script)
                {
                    var error = string.Format(
                        "Could not create script from id {0}.",
                        scriptId);

                    Log.Error(this, error);

                    tokens[i] = new MutableAsyncToken<SpireScript>(new Exception(
                        error));

                    continue;
                }

                tokens[i] = script.OnReady;
            }

            var token = new AsyncToken<SpireScript[]>();

            Async
                .All(tokens)
                .OnSuccess(token.Succeed)
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Called when the assembler has completed seting up the asset.
        /// </summary>
        private void Assembler_OnAssemblyComplete(GameObject instance)
        {
            // parent + orient
            instance.name = _srcAssetProp.Value;
            instance.transform.SetParent(GameObject.transform, false);
            instance.SetActive(true);

            // setup collider
            var bounds = _assembler.Bounds;
            var collider = EditCollider;
            if (null != collider)
            {
                collider.center = bounds.center;
                collider.size = bounds.size;
            }

            _onAssetLoaded.Succeed(this);
        }

        /// <summary>
        /// Called when the asset src changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void AssetSrc_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateAsset();
        }

        /// <summary>
        /// Called when the scripts src changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Scripts_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            RefreshScripts();
        }
    }
}