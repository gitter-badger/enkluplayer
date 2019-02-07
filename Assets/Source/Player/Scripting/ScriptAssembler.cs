using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Newtonsoft.Json.Linq;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptAssembler : IScriptAssembler
    {
        /// <summary>
        /// The schema key where scripts are stored.
        /// </summary>
        private const string SCHEMA_SOURCE = "scripts";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IScriptManager _scriptManager;
        private readonly IScriptFactory _scriptFactory;
        private readonly IElementJsCache _jsCache;
        private readonly IScriptRequireResolver _requireResolver;
        private readonly AppJsApi _appJsApi;

        /// <summary>
        /// The backing widget.
        /// </summary>
        private Widget _widget;
        
        /// <summary>
        /// The widget's schema prop for scripts.
        /// </summary>
        private ElementSchemaProp<string> _schemaProp;

        /// <summary>
        /// List of EnkluScripts attached to this object.
        /// </summary>
        private readonly List<EnkluScript> _scripts = new List<EnkluScript>();
        
        /// <summary>
        /// Map of built Script components.
        /// </summary>
        private readonly Dictionary<EnkluScript, Script> _scriptComponents = new Dictionary<EnkluScript, Script>();

        /// <summary>
        /// Backing scripting engine.
        /// </summary>
        private UnityScriptingHost _engine;
 
        /// <inheritdoc />
        public event Action<Script[], Script[]> OnScriptsUpdated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptAssembler(
            IScriptManager scriptManager, 
            IScriptFactory scriptFactory, 
            IElementJsCache jsCache,
            IScriptRequireResolver requireResolver,
            AppJsApi appJsApi)
        {
            _scriptManager = scriptManager;
            _scriptFactory = scriptFactory;
            _jsCache = jsCache;
            _requireResolver = requireResolver;
            _appJsApi = appJsApi;
        }

        /// <inheritdoc />
        public void Setup(Widget widget)
        {
            Log.Info(this, "Loading scripts for {0}", widget);
            _widget = widget;
            _engine = CreateBehaviorHost();

            _schemaProp = widget.Schema.GetOwn(SCHEMA_SOURCE, "[]");
            _schemaProp.OnChanged += Schema_OnUpdated;

            var contentWidget = widget as ContentWidget;
            if (contentWidget != null)
            {
                contentWidget.OnAssetLoaded.OnFinally(_ => CreateScripts());
            }
            else
            {
                CreateScripts();
            }
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _engine = null;

            _schemaProp.OnChanged -= Schema_OnUpdated;

            for (int i = 0, len = _scripts.Count; i < len; i++)
            {
                _scripts[i].OnUpdated -= Script_OnUpdated;
                _scriptManager.Release(_scripts[i]);
            }

            _scripts.Clear();
            _scriptComponents.Clear();
        }

        
        private void CreateScripts()
        {
            var ids = GetScriptIds();
            var idsLen = ids.Length;
            Log.Info(this, "\tLoading {0} scripts.", idsLen);

            // Check for removals
            for (var i = _scripts.Count - 1; i >= 0; i--)
            {
                if (!ids.Contains(_scripts[i].Data.Id))
                {
//                    _scriptComponents.Remove(_scripts[i]);
//                    _scripts.RemoveAt(i);
                }
            }
            
            // Create scripts
            for (var i = 0; i < idsLen; i++)
            {
                var existingScript = _scripts.FirstOrDefault(s => s.Data.Id == ids[i]);
                if (existingScript != null)
                {
                    continue;
                }
                
                var script = _scriptManager.Create(ids[i], ids[i], _widget.Id);
                if (script == null)
                {
                    Log.Error(this, "Unable to create script {0}", ids[i]);
                    
                    // AbortScripts ?
                    throw new Exception("Unable to create script " + ids[i]);
                }
                
                _scripts.Add(script);
                script.OnUpdated += Script_OnUpdated;
            }

            // Hook callbacks
            var loadTokens = new IAsyncToken<EnkluScript>[idsLen];
            for (var i = 0; i < idsLen; i++)
            {
                var scriptToken = new AsyncToken<EnkluScript>();
                loadTokens[i] = scriptToken;
                
                var script = _scripts[i];

                if (script.Status == EnkluScript.LoadStatus.IsLoading)
                {
                    // TODO: Don't over subscribe
                    script.OnLoadSuccess += scriptToken.Succeed;
                    script.OnLoadFailure += (s) =>
                    {
                        Log.Warning(this, "Failed to load EnkluScript ({0})", s);
                        scriptToken.Succeed(s);
                    };
                }
                else
                {
                    scriptToken.Succeed(script);
                }
            }
            
            Async.All(loadTokens).OnSuccess(scripts =>
            {
                var configurationTokens = new IAsyncToken<Script>[scripts.Length];
                var components = new List<Script>(scripts.Length);
                for (int i = 0, len = scripts.Length; i < len; i++)
                {
                    configurationTokens[i] = CreateScript(scripts[i]);
                    configurationTokens[i].OnSuccess(script => components.Add(script));
                }

                Async.All(configurationTokens).OnFinally(_ =>
                {
                    var existing = _scriptComponents.Values.ToArray();
                    
                    for (int i = 0, len = components.Count; i < len; i++)
                    {
                        _scriptComponents[components[i].EnkluScript] = components[i];
                    }
                    
                    OnScriptsUpdated.Execute(existing, _scriptComponents.Values.ToArray());
                });
            });
        }
        
        private IAsyncToken<Script> CreateScript(EnkluScript script)
        {
            Script newScript;
            
            switch (script.Data.Type)
            {
                case ScriptType.Vine:
                    newScript = _scriptFactory.Vine(_widget, script);
                    break;
                case ScriptType.Behavior:
                    newScript = _scriptFactory.Behavior(_widget, _jsCache, _engine, script);
                    break;
                default:
                    throw new Exception("Is there a new script type?!");
            }
            
            var rtnToken = new AsyncToken<Script>();
            newScript.Configure()
                .OnSuccess(_ => rtnToken.Succeed(newScript))
                .OnFailure(rtnToken.Fail);
            
            return rtnToken;
        }
        
        /// <summary>
        /// Retrieves script ids to load.
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptIds()
        {
            // unescape-- this is dumb obviously
            var scriptsSrc = _schemaProp.Value.Replace("\\\"", "\"");

            JArray value;
            try
            {
                value = JArray.Parse(scriptsSrc);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not parse \"{0}\" : {1}.",
                    scriptsSrc,
                    exception);

                return new string[0];
            }

            var len = value.Count;
            var ids = new string[len];
            for (var i = 0; i < len; i++)
            {
                ids[i] = value[i]["id"].ToObject<string>();
            }

            return ids;
        }

        /// <summary>
        /// Called when script schema changes. Scripts have been removed or added to the Widget.
        /// </summary>
        private void Schema_OnUpdated(ElementSchemaProp<string> prop, string prev, string next)
        {
            Log.Info(this, "Widget script list updated ({0})", _widget);
            CreateScripts();
        }
        
        /// <summary>
        /// Called when a script's source updates.
        /// </summary>
        private void Script_OnUpdated(EnkluScript script)
        {
            Log.Info(this, "Updating script ({0} {1})", script.Data.Id, _widget);
            // Find existing script
            var existing = _scriptComponents.FirstOrDefault(
                kvp => kvp.Value.EnkluScript.Data.Id == script.Data.Id).Value;
            
            if (existing == null)
            {
                throw new Exception("No prior script when updating!");
            }

            var oldScripts = _scriptComponents.Values.ToArray();
            
            CreateScript(script)
                .OnSuccess(newScript =>
                {
                    Log.Info(this, "Script updated.");
                    _scriptComponents[script] = newScript;
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Error updating script. {0}", exception);
                    _scriptComponents[script] = null;
                })
                .OnFinally(_ =>
                {
                    OnScriptsUpdated.Execute(oldScripts, _scriptComponents.Values.ToArray());
                });
        }

        /// <summary>
        /// Creates a UnityScriptingHost with all the works.
        /// </summary>
        /// <returns></returns>
        private UnityScriptingHost CreateBehaviorHost()
        {
            var host = new UnityScriptingHost(_widget, _requireResolver, _scriptManager);
            host.SetValue("system", SystemJsApi.Instance);
            host.SetValue("app", _appJsApi);
            host.SetValue("this", _jsCache.Element(_widget));
            
            return host;
        }
    }
}