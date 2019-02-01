using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint.Parser.Ast;
using Newtonsoft.Json.Linq;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptRunner
    {
        public enum RunnerState
        {
            None,
            Loading,
            Parsing,
            Idle,
            Running
        }
        
        public enum SetupState
        {
            Error = -1,
            None,
            Loading,
            Loaded,
            Parsing,
            Done
        }

        private class WidgetRecord
        {
            public Widget Widget;
            
            public readonly List<EnkluScript> Scripts = new List<EnkluScript>();
            public readonly List<VineScript> Vines = new List<VineScript>();
            public readonly List<BehaviorScript> Behaviors = new List<BehaviorScript>();

            public SetupState SetupState;
            public UnityScriptingHost Engine;

            public ElementSchemaProp<string> ScriptSchema;
            public IAsyncToken<Void[]> LoadToken;
        }

        private const string SCHEMA_SOURCE = "scripts";
        
        private readonly IScriptManager _scriptManager;
        private readonly IScriptFactory _scriptFactory;
        private readonly IScriptRequireResolver _requireResolver;
        private readonly IElementJsCache _jsCache;
        private readonly AppJsApi _appJsApi;
        
        private readonly List<WidgetRecord> _widgetRecords = new List<WidgetRecord>();
        
        private readonly List<IAsyncToken<Void[]>> _scriptLoading = new List<IAsyncToken<Void[]>>();
        private readonly List<IAsyncToken<Void>> _vineTokens = new List<IAsyncToken<Void>>();
        private readonly AsyncToken<Void> _vinesComplete = new AsyncToken<Void>();

        private RunnerState _runnerState;

        public ScriptRunner(
            IScriptManager scriptManager, 
            IScriptFactory scriptFactory,
            IScriptRequireResolver requireResolver,
            IElementJsCache jsCache,
            AppJsApi appJsApi)
        {
            _scriptManager = scriptManager;
            _scriptFactory = scriptFactory;
            _requireResolver = requireResolver;
            _jsCache = jsCache;
            _appJsApi = appJsApi;
        }
        
        public void AddWidget(Widget widget)
        {
            if (_widgetRecords.Any(rec => rec.Widget == widget))
            {
                throw new Exception("Widget already added.");
            }
            var record = new WidgetRecord
            {
                Widget = widget,
                SetupState = SetupState.None,
                Engine = CreateBehaviorHost(widget)
            };
            _widgetRecords.Add(record);
            
            HookSchema(record);
            LoadScripts(record);

            // TODO: Handle AddWidget during load/parse
            if (_runnerState != RunnerState.None)
            {
                ParseWidget(record);
            }
            else
            {
                _scriptLoading.Add(record.LoadToken);
            }
        }

        public SetupState GetSetupState(Widget widget)
        {
            var record = _widgetRecords.FirstOrDefault(rec => rec.Widget == widget);

            if (record == null)
            {
                throw new Exception("Unknown widget.");
            }
            
            return record.SetupState;
        }

        public IAsyncToken<Void> ParseAll()
        {
            Log.Warning(this, "ParseAll");
            
            if (_runnerState != RunnerState.None)
            {
                throw new Exception("ParseAll is invalid. Already setup.");
            }
            Log.Info(this, "Script parsing requested.");

            var rtnToken = new AsyncToken<Void>();
            Exception exception = null;
            Async.All(_scriptLoading.ToArray()).OnFinally((_) =>
            {
                Log.Info(this, "Scripts loaded. Starting parsing.");

                for (int i = 0, len = _widgetRecords.Count; i < len; i++)
                {
                    ParseWidget(_widgetRecords[i]);
                }

                Async.All(_vineTokens.ToArray()).OnSuccess((__) =>
                {
                    Log.Info(this, "Vines parsed. Parsing Behaviors.");
                    
                    // Currently, this triggers behavior parsing/entering
                    _vinesComplete.Succeed(Void.Instance);

                    for (int i = 0, len = _widgetRecords.Count; i < len; i++)
                    {
                        // TODO: Don't blindly assume success
                        _widgetRecords[i].SetupState = SetupState.Done;
                    }

                    Log.Info(this, "All parsing complete.");

                    exception = null;
                })
                .OnFinally(__ =>
                {
                    // Delay resolving rtnToken until finally, to ensure the proper
                    // RunnerState is set before external calls may try to interact with ScriptRunner again.
                    _runnerState = RunnerState.Idle;
                    if (exception == null)
                    {
                        rtnToken.Succeed(Void.Instance);
                    }
                    else
                    {
                        rtnToken.Fail(exception);
                    }
                });
            });
            return rtnToken;
        }

        private void ParseWidget(WidgetRecord record)
        {   
            if (record.SetupState != SetupState.Loaded)
            {
                throw new Exception("Should this be an exception?");
            }
            record.SetupState = SetupState.Parsing;

            record.LoadToken.OnSuccess(_ =>
            {
                var vineTokens = ParseVines(record);

                if (_runnerState == RunnerState.None)
                {
                    for (int i = 0, len = vineTokens.Count; i < len; i++)
                    {
                        _vineTokens.Add(vineTokens[i]);
                    }

                    ExecuteBehaviors(record, _vinesComplete);
                }
                else
                {
                    var allToken = Async.All(vineTokens.ToArray());
                    ExecuteBehaviors(record, Async.Map(allToken, __ => Void.Instance));
                }
            });
        }

        public void StartAllScripts()
        {
            Log.Warning(this, "StartAllScripts");

            if (_runnerState == RunnerState.Running)
            {
                throw new Exception("Scripts already running!");
            }

            _runnerState = RunnerState.Running;

            var len = _widgetRecords.Count;
            for (var i = 0; i < len; i++)
            {
                var vineComponents = _widgetRecords[i].Vines;

                for (int j = 0, jLen = vineComponents.Count; j < jLen; j++)
                {
                    StartScript(vineComponents[j]);
                }
            }
            
            for (var i = 0; i < len; i++)
            {
                var behaviorComponents = _widgetRecords[i].Behaviors;

                for (int j = 0, jLen = behaviorComponents.Count; j < jLen; j++)
                {
                    StartScript(behaviorComponents[j]);
                }
            }
        }

        public void StopAllScripts()
        {
            Log.Warning(this, "StopAllScripts");
            
            if (_runnerState != RunnerState.Running)
            {
                throw new Exception("Scripts weren't running.");
            }

            _runnerState = RunnerState.Idle;

            var len = _widgetRecords.Count;
            for (var i = 0; i < len; i++)
            {
                var vineComponents = _widgetRecords[i].Vines;

                for (int j = 0, jLen = vineComponents.Count; j < jLen; j++)
                {
                    StopScript(vineComponents[j]);
                }
            }
            
            for (var i = 0; i < len; i++)
            {
                var behaviorComponents = _widgetRecords[i].Behaviors;

                for (int j = 0, jLen = behaviorComponents.Count; j < jLen; j++)
                {
                    StopScript(behaviorComponents[j]);
                }
            }
        }

        public void Update()
        {
            if (_runnerState != RunnerState.Running)
            {
                return;
            }
            
            // Vines currently have no Update usage, so skip for now.
            
            for (int i = 0, len = _widgetRecords.Count; i < len; i++)
            {
                var behaviorComponents = _widgetRecords[i].Behaviors;

                for (int j = 0, jLen = behaviorComponents.Count; j < jLen; j++)
                {
                    behaviorComponents[j].FrameUpdate();
                }
            }
        }

        private void StartWidget(WidgetRecord record)
        {
            for (int i = 0, len = record.Vines.Count; i < len; i++)
            {
                StartScript(record.Vines[i]);
            }
            
            for (int i = 0, len = record.Behaviors.Count; i < len; i++)
            {
                StartScript(record.Behaviors[i]);
            }
        }

        private void StopWidget(WidgetRecord record)
        {
            for (int i = 0, len = record.Vines.Count; i < len; i++)
            {
                StopScript(record.Vines[i]);
            }
            
            for (int i = 0, len = record.Behaviors.Count; i < len; i++)
            {
                StopScript(record.Behaviors[i]);
            }
        }

        private void StartScript(Script script)
        {
            if (!script.IsConfigured || script.IsRunning)
            {
                return;
            }
            
            try
            {
                script.Enter();
            }
            catch (Exception e)
            {
                Log.Error(this, "Error entering Script: {0}", e);
            }
        }

        private void StopScript(Script script)
        {
            if (!script.IsConfigured || !script.IsRunning)
            {
                return;
            }
            
            try
            {
                script.Exit();
            }
            catch (Exception e)
            {
                Log.Error(this, "Error exiting Script: {0}", e);
            }
        }

        private UnityScriptingHost CreateBehaviorHost(Widget widget)
        {
            var host = new UnityScriptingHost(widget, _requireResolver, _scriptManager);
            host.SetValue("system", SystemJsApi.Instance);
            host.SetValue("app", _appJsApi);
            host.SetValue("this", _jsCache.Element(widget));
            
            return host;
        }

        private void HookSchema(WidgetRecord record)
        {
            record.ScriptSchema = record.Widget.Schema.GetOwn(SCHEMA_SOURCE, "[]");
            record.ScriptSchema.OnChanged += (prop, s, arg3) =>
            {
                Log.Info(this, "Widget script list changed.");
                StopWidget(record);
                LoadScripts(record);
                ParseWidget(record);
            };
        }

        private IAsyncToken<Void[]> CreateScripts(WidgetRecord record)
        {
            record.SetupState = SetupState.Loading;
            
            var ids = GetScriptIds(record);
            var idsLen = ids.Length;
            Log.Info(this, "\tLoading {0} scripts.", idsLen);

            // Create scripts
            var scripts = record.Scripts;
            var scriptsLen = scripts.Count;
            for (var i = 0; i < idsLen; i++)
            {
                var newScript = true;
                // Ignore existing scripts
                for (var j = 0; j < scriptsLen; j++)
                {
                    if (scripts[j].Data.Id == ids[i])
                    {
                        newScript = false;
                        break;
                    }
                }

                if (!newScript)
                {
                    continue;
                }
                
                var script = _scriptManager.Create(ids[i], ids[i], record.Widget.Id);
                if (script == null)
                {
                    Log.Error(this, "Unable to create script {0}", ids[i]);
                    
                    // AbortScripts ?
                    throw new Exception("Unable to create script " + ids[i]);
                }
                
                scripts.Add(script);
            }

            // Hook callbacks
            var scriptTokens = new IAsyncToken<Void>[idsLen];
            for (var i = 0; i < idsLen; i++)
            {
                var scriptToken = new AsyncToken<Void>();
                scriptTokens[i] = scriptToken;
                
                var script = scripts[i];

                if (script.Status == EnkluScript.LoadStatus.IsLoading)
                {
                    // TODO: Don't over subscribe
                    script.OnLoadSuccess += (_) => scriptToken.Succeed(Void.Instance);
                    script.OnLoadFailure += (_) => scriptToken.Succeed(Void.Instance);
                }
                else
                {
                    scriptToken.Succeed(Void.Instance);
                }
                
                script.OnUpdated += (_) => Script_OnUpdated(record.Widget, script);
            }
            

            return Async.All(scriptTokens)
                .OnSuccess(_ => record.SetupState = SetupState.Loaded);
        }

        private void Script_OnUpdated(Widget widget, EnkluScript script)
        {
            var record = _widgetRecords.First(rec => rec.Widget == widget); 
            
            // Find existing script
            Script existing = null;
            Script newScript;
            
            switch (script.Data.Type)
            {
                case ScriptType.Vine:
                    existing = record.Vines.FirstOrDefault(v => v.EnkluScript.Data.Id == script.Data.Id);
                    newScript = _scriptFactory.Vine(widget, script);
                    break;
                case ScriptType.Behavior:
                    existing = record.Behaviors.FirstOrDefault(b => b.EnkluScript.Data.Id == script.Data.Id);
                    newScript = _scriptFactory.Behavior(widget, _jsCache, record.Engine, script);
                    break;
                default:
                    throw new Exception("Is there a new script type?!");
            }

            if (existing == null)
            {
                throw new Exception("No prior script when updating!");
            }

            newScript.Configure()
                .OnSuccess(_ =>
                {
                    Log.Info(this, "Swapping script");

                    Log.Info(this, _runnerState);
                    if (_runnerState == RunnerState.Running)
                    {
                        StopWidget(record);
                    }
                    
                    RemoveScript(record, existing);
                    AddScript(record, newScript);

                    if (_runnerState == RunnerState.Running)
                    {
                        StartWidget(record);
                    }
                })
                .OnFailure(_ =>
                {
                    Log.Error(this, "Error creating new script.");
                });
        }
        
        /// <summary>
        /// Retrieves script ids to load.
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptIds(WidgetRecord record)
        {
            // unescape-- this is dumb obviously
            var scriptsSrc = record.ScriptSchema.Value.Replace("\\\"", "\"");

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

        private void LoadScripts(WidgetRecord record)
        {
            try
            {
                record.LoadToken = CreateScripts(record);
            }
            catch (Exception e)
            {
                Log.Error("Error running scripts for {0} : {1}", record.Widget, e);
                record.SetupState = SetupState.Error;
            }
        }

        /// <summary>
        /// Adds a script to a widget's record.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="script"></param>
        private void AddScript(WidgetRecord record, Script script)
        {
            switch (script.EnkluScript.Data.Type)
            {
                case ScriptType.Vine:
                    record.Vines.Add((VineScript) script);
                    break;
                case ScriptType.Behavior:
                    record.Behaviors.Add((BehaviorScript) script);
                    break;
            }
        }
        
        /// <summary>
        /// Removes a script from a Widget's record.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="script"></param>
        private void RemoveScript(WidgetRecord record, Script script)
        {
            switch (script.EnkluScript.Data.Type)
            {
                case ScriptType.Vine:
                    record.Vines.Remove((VineScript) script);
                    break;
                case ScriptType.Behavior:
                    record.Behaviors.Remove((BehaviorScript) script);
                    break;
            }
        }

        /// <summary>
        /// Sets up Vine instances for a given WidgetRecord. Vine configuration tokens are returned.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private List<IAsyncToken<Void>> ParseVines(WidgetRecord record)
        {
            List<IAsyncToken<Void>> tokens = new List<IAsyncToken<Void>>();

            for (int i = 0, len = record.Scripts.Count; i < len; i++)
            {
                var script = record.Scripts[i];
                if (script.Data.Type != ScriptType.Vine)
                {
                    continue;
                }

                Log.Info(this, "ParseVines");
                var component = _scriptFactory.Vine(record.Widget, script);
                AddScript(record, component);
                
                tokens.Add(component.Configure().OnSuccess((_) =>
                {
                    // TODO: Defer this until ScriptService calls
                    if (_runnerState == RunnerState.Running)
                    {
//                        StopScript(component);
                        StartScript(component);
                    }
                }));
            }

            return tokens; 
        }

        /// <summary>
        /// Sets up Behavior instances for a given WidgetRecord.
        /// All configuration happens on success of the trigger token.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="triggerToken"></param>
        private void ExecuteBehaviors(WidgetRecord record, IAsyncToken<Void> triggerToken)
        {
            for (int i = 0, len = record.Scripts.Count; i < len; i++)
            {
                var script = record.Scripts[i];
                if (script.Data.Type != ScriptType.Behavior)
                {
                    continue;
                }

                var component = _scriptFactory.Behavior(record.Widget, _jsCache, record.Engine, script);
                AddScript(record, component);
                
                triggerToken.OnSuccess((_) =>
                {
                    component.Configure();
                    if (_runnerState == RunnerState.Running)
                    {
                        StopScript(component);
                        StartScript(component);
                    }
                });
            }
        }
    }
}