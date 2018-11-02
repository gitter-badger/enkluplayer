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
    public class ScriptRunner
    {
        public enum SetupState
        {
            Error = -1,
            None,
            Parsing,
            Done
        }

        private const string SCHEMA_SOURCE = "scripts";
        
        private readonly IScriptManager _scriptManager;
        private readonly IScriptFactory _scriptFactory;
        private readonly IScriptRequireResolver _requireResolver;
        private readonly IElementJsCache _jsCache;

        private readonly Dictionary<Widget, List<EnkluScript>> _scriptLookup = new Dictionary<Widget, List<EnkluScript>>();
        private readonly Dictionary<Widget, SetupState> _setupStates = new Dictionary<Widget, SetupState>();

        private readonly Dictionary<Widget, List<VineScript>> _vines = new Dictionary<Widget, List<VineScript>>();

        private readonly Dictionary<Widget, List<BehaviorScript>> _behaviors = new Dictionary<Widget, List<BehaviorScript>>();

        private readonly Dictionary<Widget, UnityScriptingHost> _engines = new Dictionary<Widget, UnityScriptingHost>();
        
        private readonly List<IAsyncToken<Void>> _vineTokens = new List<IAsyncToken<Void>>();
        private readonly AsyncToken<Void> _vinesComplete = new AsyncToken<Void>();

        public ScriptRunner(
            IScriptManager scriptManager, 
            IScriptFactory scriptFactory,
            IScriptRequireResolver requireResolver,
            IElementJsCache jsCache)
        {
            _scriptManager = scriptManager;
            _scriptFactory = scriptFactory;
            _requireResolver = requireResolver;
            _jsCache = jsCache;
        }
        
        public void AddWidget(Widget widget)
        {
            if (_scriptLookup.ContainsKey(widget))
            {
                throw new Exception("Widget already added.");
            }
            
            _scriptLookup.Add(widget, new List<EnkluScript>());
            _vines.Add(widget, new List<VineScript>());
            _behaviors.Add(widget, new List<BehaviorScript>());
            _engines.Add(widget, CreateBehaviorHost(widget));
            _setupStates.Add(widget, SetupState.Parsing);

            try
            {
                CreateScripts(widget).OnSuccess((_) => _setupStates[widget] = SetupState.Done);
            }
            catch (Exception e)
            {
                Log.Error("Error running scripts for {0}", widget);
                _setupStates[widget] = SetupState.Error;
            }
        }

        public SetupState GetSetupState(Widget widget)
        {
            if (!_setupStates.ContainsKey(widget))
            {
                throw new Exception("Unknown widget.");
            }

            return _setupStates[widget];
        }

        public void Parse()
        {
            foreach (var kvp in _scriptLookup)
            {
                var widget = kvp.Key;
                var scripts = kvp.Value;
                
                for (int i = 0, len = scripts.Count; i < len; i++)
                {
                    switch (scripts[i].Data.Type)
                    {
                        case ScriptType.Vine:
                        {
                            var vineComponent = _scriptFactory.Vine(widget, scripts[i]);
                        
                            _vines[widget].Add(vineComponent);
                            _vineTokens.Add(vineComponent.Configure().OnSuccess((_) =>
                            {
                                // TODO: Defer this until ScriptService calls
                                StartScript(vineComponent);
                            }));
                            break;
                        }
                        case ScriptType.Behavior:
                        {
                            var behaviorComponent = _scriptFactory.Behavior(
                                widget, _jsCache, _engines[widget], scripts[i]);
                            
                            _behaviors[widget].Add(behaviorComponent);
                            _vinesComplete.OnFinally((_) =>
                            {
                                behaviorComponent.Configure();
                                
                                // TODO: Defer this until ScriptService calls
                                StartScript(behaviorComponent);
                            });
                            break;
                        }
                    }
                }
            }

            Async.All(_vineTokens.ToArray()).OnSuccess((_) =>
            {
                _vinesComplete.Succeed(Void.Instance);

                var keys = _setupStates.Keys.ToList();
                for (int i = 0, len = keys.Count; i < len; i++)
                {
                    // TODO: Don't blindly assume success
                    _setupStates[keys[i]] = SetupState.Done;
                }
            });
        }

        public void StartScripts()
        {
            foreach (var kvp in _vines)
            {
                var vineComponents = kvp.Value;

                for (int i = 0, len = vineComponents.Count; i < len; i++)
                {
                    StartScript(vineComponents[i]);
                }
            }

            foreach (var kvp in _behaviors)
            {
                var behaviorComponents = kvp.Value;

                for (int i = 0, len = behaviorComponents.Count; i < len; i++)
                {
                    StartScript(behaviorComponents[i]);
                }
            }
        }

        private void StartScript(Script script)
        {
            try
            {
                script.Enter();
            }
            catch (Exception e)
            {
                Log.Error("Error entering Script: {0}", e);
            }
        }

        private UnityScriptingHost CreateBehaviorHost(Widget widget)
        {
            return new UnityScriptingHost(widget, _requireResolver, _scriptManager);
        }

        private IAsyncToken<Void[]> CreateScripts(Widget widget)
        {
            var ids = GetScriptIds(widget);
            var len = ids.Length;
            Log.Info(this, "\tLoading {0} scripts.", len);

            List<EnkluScript> scripts = _scriptLookup[widget];
            
            for (var i = 0; i < len; i++)
            {
                var script = _scriptManager.Create(ids[i], ids[i], widget.Id);
                if (script == null)
                {
                    Log.Error(this, "Unable to create script {0}", ids[i]);
                    
                    // AbortScripts ?
                    throw new Exception("Unable to create script " + ids[i]);
                }
                
                scripts.Add(script);
                // TODO: Use script.status
            }

            var scriptTokens = new IAsyncToken<Void>[len];

            for (var i = 0; i < len; i++)
            {
                var scriptToken = new AsyncToken<Void>();
                scriptTokens[i] = scriptToken;
                
                var script = scripts[i];
                script.OnLoadSuccess += (_) => scriptToken.Succeed(Void.Instance);
                script.OnLoadFailure += (_) => scriptToken.Succeed(Void.Instance);
//                script.OnUpdated += Script_OnUpdated;
            }

            return Async.All(scriptTokens);
        }
        
        /// <summary>
        /// Retrieves script ids to load.
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptIds(Widget widget)
        {
            var scriptsSrc = widget.Schema.GetOwn(SCHEMA_SOURCE, "[]").Value;

            // unescape-- this is dumb obviously
            scriptsSrc = scriptsSrc.Replace("\\\"", "\"");

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