﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using LightJson;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Widget that loads + holds an asset, behaviors, and vines.
    /// </summary>
    public class ContentWidget : Widget
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
        /// Provider.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Caches js objects.
        /// </summary>
        private IElementJsCache _jsCache;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _srcAssetProp;
        private ElementSchemaProp<string> _scriptsProp;

        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onAssetLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onScriptsLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Behaviors.
        /// </summary>
        private readonly List<SpireScriptElementBehavior> _scriptComponents = new List<SpireScriptElementBehavior>();
        
        /// <summary>
        /// Vines.
        /// </summary>
        private readonly List<VineMonoBehaviour> _vineComponents = new List<VineMonoBehaviour>();

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
        private bool _pollUpdateScript;

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
            IWorldAnchorProvider provider)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _scripts = scripts;
            _assembler = assembler;
            _assembler.OnAssemblyComplete += Assembler_OnAssemblyComplete;
            _provider = provider;
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _host = new UnityScriptingHost(
                this,
                null,
                _scripts);
            _jsCache = new ElementJsCache(_host);
            _host.SetValue("this", _jsCache.Element(this));

            _srcAssetProp = Schema.GetOwn("assetSrc", "");
            _srcAssetProp.OnChanged += AssetSrc_OnChanged;

            _scriptsProp = Schema.GetOwn("scripts", "[]");
            _scriptsProp.OnChanged += Scripts_OnChanged;
            
            UpdateAsset();
            UpdateScripts();
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();
            
            _srcAssetProp.OnChanged -= AssetSrc_OnChanged;
            _scriptsProp.OnChanged -= Scripts_OnChanged;

            _assembler.Teardown();
            TeardownScripts();
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_pollUpdateAsset)
            {
                UpdateAsset();
            }

            if (_pollUpdateScript)
            {
                UpdateScripts();
            }

            for (int i = 0, len = _scriptComponents.Count; i < len; i++)
            {
                _scriptComponents[i].Update();
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
        private void UpdateScripts()
        {
            _pollUpdateScript = false;

            if (!ShouldLoadAsset())
            {
                _pollUpdateScript = true;
                return;
            }

            Log.Info(this, "Refresh scripts for {0}.", Id);

            TeardownScripts();

            // TODO: reset element -- all props need reset from data

            SetupScripts();
        }
        
        /// <summary>
        /// Loads all scripts.
        /// </summary>
        private void SetupScripts()
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
                return;
            }

            var len = value.Count;

            Log.Info(this, "\tLoading {0} scripts.", len);

            if (0 == len)
            {
                _onScriptsLoaded.Succeed(this);
                return;
            }

            var scripts = new SpireScript[len];
            var tokens = new IMutableAsyncToken<SpireScript>[len];
            for (var i = 0; i < len; i++)
            {
                var data = value[i];
                var scriptId = data["id"].AsString;
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

            // when all scripts are loaded, resolve the mutable token
            Async
                .All(tokens)
                .OnSuccess(_ => _onScriptsLoaded.Succeed(this))
                .OnSuccess(_ =>
                {
                    // start all vines
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

                    // start all behaviors
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
                })
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
                _scriptComponents[i].Exit();
            }
            _scriptComponents.Clear();

            // release scripts we created
            _scripts.ReleaseAll(_scriptTag);
        }

        /// <summary>
        /// Runs a vine script
        /// </summary>
        /// <param name="script">The vine to run.</param>
        private void RunVine(SpireScript script)
        {
            Log.Info(this, "Run vine : {0}", script.Data.Id);

            VineMonoBehaviour component = null;

            var found = false;
            for (int j = 0, jlen = _vineComponents.Count; j < jlen; j++)
            {
                component = _vineComponents[j];
                if (component.Script.Data.Id == script.Data.Id)
                {
                    component.Exit();

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                component = GameObject.AddComponent<VineMonoBehaviour>();
                _vineComponents.Add(component);
            }

            if (component.Initialize(script))
            {
                component.Enter();
            }
        }

        /// <summary>
        /// Runs a behavior script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        private void RunBehavior(SpireScript script)
        {
            Log.Info(this, "RunBehavior({0}) : {1}", script.Data, script.Source);

            // restart or create new component
            SpireScriptElementBehavior component = null;

            var found = false;
            for (int j = 0, jlen = _scriptComponents.Count; j < jlen; j++)
            {
                component = _scriptComponents[j];
                if (component.Script.Data.Id == script.Data.Id)
                {
                    component.Exit();

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                component = new SpireScriptElementBehavior();
                _scriptComponents.Add(component);
            }

            component.Initialize(_host, script, this);
            component.Enter();
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
            UpdateScripts();
        }
    }
}