using System;
using System.Collections.Generic;
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
        private readonly IElementJsFactory _elementJsFactory;
        private readonly Engine _host;
        private readonly GameObject _root;
        private readonly Element _element;

        private bool _isSetup;

        /// <summary>
        /// Behaviors.
        /// </summary>
        private readonly Boo.Lang.List<SpireScriptElementBehavior> _scriptComponents = new Boo.Lang.List<SpireScriptElementBehavior>();

        /// <summary>
        /// Vines.
        /// </summary>
        private readonly Boo.Lang.List<VineMonoBehaviour> _vineComponents = new Boo.Lang.List<VineMonoBehaviour>();
        
        public ScriptCollectionRunner(
            IElementJsFactory elementJsFactory,
            Engine host,
            GameObject root,
            Element element)
        {
            _elementJsFactory = elementJsFactory;
            _host = host;
            _root = root;
            _element = element;
        }

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
        /// Lookup from script id to load status.
        /// </summary>
        private readonly Dictionary<string, SpireScript.LoadStatus> _scriptLoadMap = new Dictionary<string, SpireScript.LoadStatus>();

        /// <summary>
        /// SpireScripts we are currently running.
        /// </summary>
        private readonly List<SpireScript> _spireScripts = new List<SpireScript>();

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
        /// Retrieves script ids to load.
        /// </summary>
        /// <returns></returns>
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

            Log.Info(this, "Refresh all scripts for {0}.", Id);

            if (null == _runner)
            {
                _runner = new ScriptCollectionRunner(
                    _elementJsFactory,
                    _host,
                    GameObject,
                    this);
            }

            AbortScripts();

            // TODO: reset element -- all props need reset from data

            LoadScripts();
        }
        
        /// <summary>
        /// Aborts load, stops scripts, destroys scripts.
        /// </summary>
        private void AbortScripts()
        {
            // remove all handlers
            for (int i = 0, len = _spireScripts.Count; i < len; i++)
            {
                var script = _spireScripts[i];
                script.OnLoadStarted -= Script_OnLoadStarted;
                script.OnLoadFailure -= Script_OnLoadFinished;
                script.OnLoadSuccess -= Script_OnLoadFinished;
            }
            _spireScripts.Clear();

            // stop running scripts
            _runner.Teardown();

            // release scripts we created
            _scripts.ReleaseAll(_scriptTag);
        }
        
        /// <summary>
        /// Loads all scripts and watches for updates.
        /// </summary>
        private void LoadScripts()
        {
            _scriptLoadMap.Clear();

            var ids = GetScriptIds();
            var len = ids.Length;

            Log.Info(this, "\tLoading {0} scripts.", len);

            if (0 == len)
            {
                PollScriptLoads();
                return;
            }

            // create scripts
            for (var i = 0; i < len; i++)
            {
                var script = _scripts.Create(ids[i], _scriptTag);
                if (null == script)
                {
                    Log.Error(this, "Could not create script.");
                    
                    AbortScripts();

                    return;
                }

                _spireScripts.Add(script);
                _scriptLoadMap[script.Data.Id] = script.Status;
            }

            // listen to scripts
            for (var i = 0; i < len; i++)
            {
                var script = _spireScripts[i];

                script.OnLoadSuccess += Script_OnLoadFinished;
                script.OnLoadFailure += Script_OnLoadFinished;
                script.OnLoadStarted += Script_OnLoadStarted;
            }
        }

        /// <summary>
        /// Runs through all script loads to determine if scripts can run or not.
        /// </summary>
        private void PollScriptLoads()
        {
            foreach (var pair in _scriptLoadMap)
            {
                if (pair.Value == SpireScript.LoadStatus.Failed)
                {
                    AbortScripts();

                    return;
                }

                if (pair.Value == SpireScript.LoadStatus.IsLoading || pair.Value == SpireScript.LoadStatus.None)
                {
                    return;
                }
            }

            Log.Info(this, "All scripts loaded. Running!");

            _onScriptsLoaded.Succeed(this);

            _runner.Setup(_spireScripts);
        }

        /// <summary>
        /// Called when a script has started to load itself.
        /// </summary>
        /// <param name="script">The script in question.</param>
        private void Script_OnLoadStarted(SpireScript script)
        {
            // stop scripts NOW, request refresh
            AbortScripts();
            RefreshScripts();
        }

        /// <summary>
        /// Called when a script has either successfull or unsuccessfully loaded.
        /// </summary>
        /// <param name="script">The script in question.</param>
        private void Script_OnLoadFinished(SpireScript script)
        {
            _scriptLoadMap[script.Data.Id] = script.Status;

            PollScriptLoads();
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