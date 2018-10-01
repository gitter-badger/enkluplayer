using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using LightJson;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Widget that loads + holds an asset, behaviors, and vines.
    /// </summary>
    public class ContentWidget : Widget
    {
        /// <summary>
        /// Resolver for JsApi `requires` functionality.
        /// </summary>
        private readonly IScriptRequireResolver _resolver;

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
        private readonly Dictionary<string, EnkluScript.LoadStatus> _scriptLoadMap = new Dictionary<string, EnkluScript.LoadStatus>();

        /// <summary>
        /// EnkluScripts we are currently running.
        /// </summary>
        private readonly List<EnkluScript> _enkluScripts = new List<EnkluScript>();

        /// <summary>
        /// Loads and executes scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Assembles an asset.
        /// </summary>
        private readonly IAssetAssembler _assembler;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementJsFactory _elementJsFactory;
        
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
        /// Cached from the callback of the <see cref="IAssetAssembler"/>.
        /// </summary>
        private GameObject _assetGameObject;
        
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
        [Construct]
        public ContentWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IAssetAssembler assembler,
            IScriptRequireResolver resolver,
            IScriptManager scripts,
            IElementJsFactory elementFactory )
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _resolver = resolver;
            _scripts = scripts;
            _assembler = assembler;
            _elementJsFactory = elementFactory;
        }

        /// <summary>
        /// Constructor used for testing.
        /// </summary>
        public ContentWidget(
            GameObject gameObject, 
            IScriptManager scripts, 
            IAssetAssembler assembler)
            : base(gameObject, null, null, null)
        {
            _scripts = scripts;
            _assembler = assembler;
        }

        /// <summary>
        /// Tears down scripts and sets them back up.
        /// </summary>
        public void RefreshScripts()
        {
            _pollRefreshScript = false;

            if (!ShouldLoadScripts())
            {
                _pollRefreshScript = true;
                return;
            }

            Log.Info(this, "Refresh all scripts for {0}.", Id);

            if (null == _runner)
            {
                _runner = new ScriptCollectionRunner(
                    _scripts,
                    _resolver,
                    _elementJsFactory,
                    GameObject,
                    this);
            }

            AbortScripts();

            // TODO: reset element -- all props need reset from data

            LoadScripts();
        }

        /// <summary>
        /// Attempts to get <c>T</c> from the underlying asset's GameObject.
        /// Throws NullReferenceException if the assembly hasn't finished yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
        {
            if (_assetGameObject)
            {
                return _assetGameObject.GetComponent<T>();
            }

            return default(T);
        }

        /// <summary>
        /// Attempts to get <c>T</c> from the underlying asset's GameObject & children.
        /// Throws NullReferenceException if the assembly hasn't finished yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInChildren<T>()
        {
            if (_assetGameObject)
            {
                return _assetGameObject.GetComponentInChildren<T>();
            }

            return default(T);
        }

        /// <inheritdoc />
        protected override void DestroyInternal()
        {
            _assembler.OnAssemblyUpdated -= Assembler_OnAssemblyUpdated;

            base.DestroyInternal();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _assembler.OnAssemblyUpdated += Assembler_OnAssemblyUpdated;

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
        /// Determines if this piece of content should load its scripts.
        /// </summary>
        /// <returns></returns>
        private bool ShouldLoadScripts()
        {
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
        /// Aborts load, stops scripts, destroys scripts.
        /// </summary>
        private void AbortScripts()
        {
            // remove all handlers
            for (int i = 0, len = _enkluScripts.Count; i < len; i++)
            {
                var script = _enkluScripts[i];
                script.OnUpdated -= Script_OnUpdated;
                script.OnLoadFailure -= Script_OnLoadFinished;
                script.OnLoadSuccess -= Script_OnLoadFinished;
            }
            _enkluScripts.Clear();

            // stop running scripts
            if (null != _runner)
            {
                _runner.Teardown();
            }

            // release scripts we created
            _scripts.ReleaseAll(Id);
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
                var script = _scripts.Create(ids[i], ids[i], Id);
                if (null == script)
                {
                    Log.Error(this, "Could not create script.");
                    
                    AbortScripts();

                    return;
                }

                _enkluScripts.Add(script);
                _scriptLoadMap[script.Data.Id] = script.Status;
            }

            // listen to scripts
            for (var i = 0; i < len; i++)
            {
                var script = _enkluScripts[i];

                script.OnLoadSuccess += Script_OnLoadFinished;
                script.OnLoadFailure += Script_OnLoadFinished;
                script.OnUpdated += Script_OnUpdated;
            }
        }

        /// <summary>
        /// Runs through all script loads to determine if scripts can run or not.
        /// </summary>
        private void PollScriptLoads()
        {
            foreach (var pair in _scriptLoadMap)
            {
                if (pair.Value == EnkluScript.LoadStatus.Failed)
                {
                    AbortScripts();

                    return;
                }

                if (pair.Value == EnkluScript.LoadStatus.IsLoading || pair.Value == EnkluScript.LoadStatus.None)
                {
                    return;
                }
            }

            Log.Info(this, "All scripts loaded. Running!");

            _onScriptsLoaded.Succeed(this);

            _runner.Setup(_enkluScripts);
        }

        /// <summary>
        /// Called when a script has started to load itself.
        /// </summary>
        /// <param name="script">The script in question.</param>
        private void Script_OnUpdated(EnkluScript script)
        {
            // stop scripts NOW, request refresh
            AbortScripts();
            RefreshScripts();
        }

        /// <summary>
        /// Called when a script has either successfull or unsuccessfully loaded.
        /// </summary>
        /// <param name="script">The script in question.</param>
        private void Script_OnLoadFinished(EnkluScript script)
        {
            _scriptLoadMap[script.Data.Id] = script.Status;

            PollScriptLoads();
        }

        /// <summary>
        /// Called when the assembler has completed seting up the asset.
        /// </summary>
        private void Assembler_OnAssemblyUpdated()
        {
            Log.Info(this, "Assembly complete.");

            // parent + orient
            _assetGameObject = _assembler.Assembly;
            _assetGameObject.name = _srcAssetProp.Value;
            _assetGameObject.transform.SetParent(GameObject.transform, false);
            _assetGameObject.SetActive(true);

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