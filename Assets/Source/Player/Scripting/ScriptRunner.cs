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
        
        private readonly Dictionary<Widget, WidgetRecord> _widgetRecords = new Dictionary<Widget, WidgetRecord>();
        
        private readonly List<IAsyncToken<Void[]>> _scriptLoading = new List<IAsyncToken<Void[]>>();
        private readonly List<IAsyncToken<Void>> _vineTokens = new List<IAsyncToken<Void>>();
        private readonly AsyncToken<Void> _vinesComplete = new AsyncToken<Void>();

        private bool _isSetup;
        private bool _isRunning;

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
            if (_widgetRecords.ContainsKey(widget))
            {
                throw new Exception("Widget already added.");
            }

            var record = new WidgetRecord
            {
                Widget = widget,
                SetupState = SetupState.None,
                Engine = CreateBehaviorHost(widget)
            };
            _widgetRecords.Add(widget, record);
            
            HookSchema(record);
            LoadScripts(record);

            if (_isSetup)
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
            if (!_widgetRecords.ContainsKey(widget))
            {
                throw new Exception("Unknown widget.");
            }
            
            return _widgetRecords[widget].SetupState;
        }

        public IAsyncToken<Void> ParseAll()
        {
            if (_isSetup)
            {
                throw new Exception("ParseAll is invalid. Already setup.");
            }
            Log.Info(this, "Script parsing requested.");

            var rtn = new AsyncToken<Void>();
            Async.All(_scriptLoading.ToArray()).OnFinally((_) =>
            {
                Log.Info(this, "Scripts loaded. Starting parsing.");
                
                foreach (var record in _widgetRecords.Values)
                {
                    ParseWidget(record);
                }

                Async.All(_vineTokens.ToArray()).OnSuccess((__) =>
                {
                    Log.Info(this, "Vines parsed. Parsing Behaviors.");
                    
                    // Currently, this triggers behavior parsing/entering
                    _vinesComplete.Succeed(Void.Instance);

                    var keys = _widgetRecords.Keys.ToList();
                    for (int i = 0, len = keys.Count; i < len; i++)
                    {
                        // TODO: Don't blindly assume success
                        _widgetRecords[keys[i]].SetupState = SetupState.Done;
                    }

                    Log.Info(this, "All parsing complete.");
                    
                    rtn.Succeed(Void.Instance);
                })
                .OnFinally(__ => { _isSetup = true; });
            });
            return rtn;
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

                if (!_isSetup)
                {
                    for (int i = 0, len = vineTokens.Count; i < len; i++)
                    {
                        _vineTokens.Add(vineTokens[i]);
                    }

                    ParseBehaviors(record, _vinesComplete);
                }
                else
                {
                    var allToken = Async.All(vineTokens.ToArray());
                    ParseBehaviors(record, Async.Map(allToken, __ => Void.Instance));
                }
            });
        }

        public void StartScripts()
        {
            _isRunning = true;
            
            foreach (var kvp in _widgetRecords)
            {
                var vineComponents = kvp.Value.Vines;

                for (int i = 0, len = vineComponents.Count; i < len; i++)
                {
                    StartScript(vineComponents[i]);
                }
            }

            foreach (var kvp in _widgetRecords)
            {
                var behaviorComponents = kvp.Value.Behaviors;

                for (int i = 0, len = behaviorComponents.Count; i < len; i++)
                {
                    StartScript(behaviorComponents[i]);
                }
            }
        }

        public void StopScripts()
        {
            _isRunning = false;

            foreach (var kvp in _widgetRecords)
            {
                var vineComponents = kvp.Value.Vines;

                for (int i = 0, len = vineComponents.Count; i < len; i++)
                {
                    StopScript(vineComponents[i]);
                }
            }

            foreach (var kvp in _widgetRecords)
            {
                var behaviorComponents = kvp.Value.Behaviors;

                for (int i = 0, len = behaviorComponents.Count; i < len; i++)
                {
                    StopScript(behaviorComponents[i]);
                }
            }
        }

        public void Update()
        {
            // Vines currently have no Update usage, so skip for now.

            foreach (var kvp in _widgetRecords)
            {
                var behaviors = kvp.Value.Behaviors;

                for (int j = 0, jLen = behaviors.Count; j < jLen; j++)
                {
                    behaviors[j].FrameUpdate();
                }
            }
        }

        private void StartScript(Script script)
        {
            if (!script.IsConfigured)
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
            if (!script.IsConfigured)
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
            return new UnityScriptingHost(widget, _requireResolver, _scriptManager);
        }

        private void HookSchema(WidgetRecord record)
        {
            record.ScriptSchema = record.Widget.Schema.GetOwn(SCHEMA_SOURCE, "[]");
            record.ScriptSchema.OnChanged += (prop, s, arg3) =>
            {
                Log.Info(this, "Widget script list changed.");
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
            var record = _widgetRecords[widget];
            
            // Find existing script
            Script existing = null;
            Script newScript;
            
            switch (script.Data.Type)
            {
                case ScriptType.Vine:
                    for (int i = 0, len = record.Vines.Count; i < len; i++)
                    {
                        if (record.Vines[i].EnkluScript.Data.Id == script.Data.Id)
                        {
                            existing = record.Vines[i];
                            break;
                        }
                    }

                    newScript = _scriptFactory.Vine(widget, script);
                    break;
                case ScriptType.Behavior:
                    for (int i = 0, len = record.Behaviors.Count; i < len; i++)
                    {
                        if (record.Behaviors[i].EnkluScript.Data.Id == script.Data.Id)
                        {
                            existing = record.Behaviors[i];
                            break;
                        }
                    }

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
                    // TODO: Reload other scripts on this widget too?
                    Log.Info(this, "Swapping script");

                    var running = existing.IsRunning;

                    if (_isRunning)
                    {
                        StopScript(existing);
                    }
                    
                    RemoveScript(record, existing);
                    AddScript(record, newScript);

                    if (_isRunning)
                    {
                        StartScript(newScript);
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

                var component = _scriptFactory.Vine(record.Widget, script);
                AddScript(record, component);
                
                tokens.Add(component.Configure().OnSuccess((_) =>
                {
                    // TODO: Defer this until ScriptService calls
                    if (_isRunning)
                    {
                        StopScript(component);
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
        private void ParseBehaviors(WidgetRecord record, IAsyncToken<Void> triggerToken)
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
                    if (_isRunning)
                    {
                        StopScript(component);
                        StartScript(component);
                    }
                });
            }
        }
    }
}