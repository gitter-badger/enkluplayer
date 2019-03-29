using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Orchid;
using Jint.Parser;
using Newtonsoft.Json.Linq;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Handles loading a set of scripts and creating instances for an Element.
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
        private readonly IScriptExecutorFactory _scriptExecutorFactory;

        /// <summary>
        /// The backing Element.
        /// </summary>
        private Element _element;

        /// <summary>
        /// Context for behavior scripts to use.
        /// </summary>
        private IJsExecutionContext _jsContext;
        
        /// <summary>
        /// The Element's schema prop for scripts.
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
 
        /// <inheritdoc />
        public event Action<Script[], Script[]> OnScriptsUpdated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptAssembler(
            IScriptManager scriptManager, 
            IScriptFactory scriptFactory,
            IScriptExecutorFactory scriptExecutorFactory)
        {
            _scriptManager = scriptManager;
            _scriptFactory = scriptFactory;
            _scriptExecutorFactory = scriptExecutorFactory;
        }

        /// <inheritdoc />
        public void Setup(Element element)
        {
            if (_element != null)
            {
                throw new Exception("Setup called twice without a Teardown inbetween");
            }

            _element = element;
            _jsContext = _scriptExecutorFactory.NewExecutionContext(_element);
            
            Log.Info(this, "Loading scripts for {0}", _element);

            _schemaProp = _element.Schema.GetOwn(SCHEMA_SOURCE, "[]");
            _schemaProp.OnChanged += Schema_OnUpdated;

            var contentWidget = _element as ContentWidget;
            if (contentWidget != null)
            {
                contentWidget.OnLoaded.OnFinally(_ => SetupScripts());
                
                if (!contentWidget.Visible) SetupScripts();
            }
            else
            {
                SetupScripts();
            }
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _schemaProp.OnChanged -= Schema_OnUpdated;

            foreach (var script in _scriptComponents.Keys)
            {
                script.OnUpdated -= Script_OnUpdated;
            }

            _scriptManager.ReleaseAll(_element.Id);
            _scriptComponents.Clear();
        }

        /// <summary>
        /// Handles creating new scripts and removing old scripts for the Element based on its schema.
        /// </summary>
        private void SetupScripts()
        {
            var currentIds = GetScriptIds();
            var idsLen = currentIds.Length;
            Log.Info(this, "\tLoading {0} scripts for {1}.", idsLen, _element);

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
                
                var script = _scriptManager.Create(currentIds[i], _element.Id);
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

            // Remove scripts that no longer are on this Element.
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

            // Create instances for scripts newly added to this Element.
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
                    newScript = _scriptFactory.Vine(_element, script);
                    break;
                case ScriptType.Behavior:
                    newScript = _scriptFactory.Behavior(_jsContext, _element, script);
                    break;
                default:
                    throw new Exception("Is there a new script type?!");
            }

            return Async.Map(newScript.Configure(), _ => newScript);
        }

        /// <summary>
        /// Called when script schema changes. Scripts have been removed or added to the Element.
        /// </summary>
        private void Schema_OnUpdated(ElementSchemaProp<string> prop, string prev, string next)
        {
            Log.Info(this, "Element script list updated ({0})", _element);
            SetupScripts();
        }
        
        /// <summary>
        /// Called when a script's source updates.
        /// </summary>
        private void Script_OnUpdated(EnkluScript script)
        {
            Log.Info(this, "Updating script ({0} {1})", script.Data.Id, _element);
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
    }
}