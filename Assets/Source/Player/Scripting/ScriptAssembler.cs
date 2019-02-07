using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint.Parser;
using Newtonsoft.Json.Linq;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Handles loading a set of scripts and creating instances for a Widget.
    /// </summary>
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
        /// List of EnkluScripts this object is loading/loaded.
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
                contentWidget.OnAssetLoaded.OnFinally(_ => SetupScripts());
            }
            else
            {
                SetupScripts();
            }
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _engine = null;

            _schemaProp.OnChanged -= Schema_OnUpdated;

            foreach (var script in _scriptComponents.Keys)
            {
                script.OnUpdated -= Script_OnUpdated;
            }

            _scriptManager.ReleaseAll(_widget.Id);
            _scriptComponents.Clear();
        }

        /// <summary>
        /// Handles creating new scripts and removing old scripts for the Widget based on its schema.
        /// </summary>
        private void SetupScripts()
        {
            var currentIds = GetScriptIds();
            var idsLen = currentIds.Length;
            Log.Info(this, "\tLoading {0} scripts.", idsLen);

            var removals = new List<EnkluScript>();
            var additions = new List<EnkluScript>();

            // Collect removals
            for (var i = _scripts.Count - 1; i >= 0; i--)
            {
                if (!currentIds.Contains(_scripts[i].Data.Id))
                {
                    removals.Add(_scripts[i]);
                    _scripts.RemoveAt(i);
                }
            }
            
            // Collect additions
            for (var i = 0; i < idsLen; i++)
            {
                if (_scripts.Any(s => s.Data.Id == currentIds[i]))
                {
                    continue;
                }
                
                // TODO: Remove ids[i] from tag registration?
                var script = _scriptManager.Create(currentIds[i], currentIds[i], _widget.Id);
                if (script == null)
                {
                    Log.Error(this, "Unable to create script {0}", currentIds[i]);
                    continue;
                }
                
                additions.Push(script);
                _scripts.Add(script);
                script.OnUpdated += Script_OnUpdated;
            }

            // Wait for all scripts to load
            var loadTokens = new IAsyncToken<EnkluScript>[_scripts.Count];

            for (int i = 0, len = _scripts.Count; i < len; i++)
            {
                var token = new AsyncToken<EnkluScript>();
                loadTokens[i] = token;

                var script = _scripts[i];
                if (script.Status == EnkluScript.LoadStatus.IsLoading)
                {
                    script.OnLoadSuccess += token.Succeed;
                    script.OnLoadFailure += (s) =>
                    {
                        Log.Warning(this, "Failed to load EnkluScript ({0})", s);
                        token.Succeed(s);
                    };
                }
                else
                {
                    token.Succeed(script);
                }
            }

            Async.All(loadTokens).OnFinally(_ => UpdateScriptInstances(removals, additions, null));
        }

        /// <summary>
        /// Invokes the OnScriptsUpdated based on script changes.
        /// </summary>
        /// <param name="removed">The EnkluScripts that were removed.</param>
        /// <param name="added">The EnkluScripts that were added.</param>
        /// <param name="updated">The EnkluScripts that were updated.</param>
        private void UpdateScriptInstances(List<EnkluScript> removed, List<EnkluScript> added, List<EnkluScript> updated)
        {
            var existingInstances = _scriptComponents.Values.ToArray();

            // Remove scripts that no longer are on this Widget.
            if (removed != null)
            {
                for (var i = 0; i < removed.Count; i++)
                {
                    _scriptComponents.Remove(removed[i]);
                }
            }

            var tokenCount = (added != null ? added.Count : 0) + (updated != null ? updated.Count : 0);
            var configurationTokens = new IAsyncToken<Script>[tokenCount];
            var tokenIndex = 0;

            // Create instances for scripts newly added to this Widget.
            if (added != null)
            {
                for (var i = 0; i < added.Count; i++)
                {
                    var index = i;
                    var token = CreateScript(added[i]);
                    configurationTokens[tokenIndex++] = token;
                    token.OnSuccess(instance => _scriptComponents[added[index]] = instance);
                }
            }
            
            // Update instances for scripts that changed.
            if (updated != null)
            {
                for (var i = 0; i < updated.Count; i++)
                {
                    var index = i;
                    var token = CreateScript(updated[i]);
                    configurationTokens[tokenIndex++] = token;
                    token.OnSuccess(instance => _scriptComponents[updated[index]] = instance);
                }
            }
            
            // Dispatch change when ready.
            Async.All(configurationTokens).OnFinally(_ =>
            {
                var newInstances = _scriptComponents.Values.ToArray();
                OnScriptsUpdated.Execute(existingInstances, newInstances);
            });
        }
        
        /// <summary>
        /// Creates a Script instance asynchronously
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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

            return Async.Map(newScript.Configure(), _ => newScript);
        }

        /// <summary>
        /// Called when script schema changes. Scripts have been removed or added to the Widget.
        /// </summary>
        private void Schema_OnUpdated(ElementSchemaProp<string> prop, string prev, string next)
        {
            Log.Info(this, "Widget script list updated ({0})", _widget);
            SetupScripts();
        }
        
        /// <summary>
        /// Called when a script's source updates.
        /// </summary>
        private void Script_OnUpdated(EnkluScript script)
        {
            Log.Info(this, "Updating script ({0} {1})", script.Data.Id, _widget);
            UpdateScriptInstances(null, null, new List<EnkluScript> { script });
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