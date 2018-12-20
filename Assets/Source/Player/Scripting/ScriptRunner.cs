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

        private class WidgetRecord
        {
            public Widget Widget;
            
            public List<EnkluScript> Scripts = new List<EnkluScript>();
            public List<VineScript> Vines = new List<VineScript>();
            public List<BehaviorScript> Behaviors = new List<BehaviorScript>();

            public SetupState SetupState;
            public UnityScriptingHost Engine;
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
            
        private SetupState _globalState = SetupState.None;

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
            
            _widgetRecords.Add(widget, new WidgetRecord()
            {
                Widget = widget,
                SetupState = SetupState.None,
                Engine = CreateBehaviorHost(widget)
            });

            if (_globalState == SetupState.Done || _globalState == SetupState.Error)
            {
                ParseWidget(widget);
            }
            else
            {
                try
                {
                    _scriptLoading.Add(CreateScripts(widget));
                }
                catch (Exception e)
                {
                    Log.Error("Error running scripts for {0} : {1}", widget, e);
                    _widgetRecords[widget].SetupState = SetupState.Error;
                }
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
            if (_globalState != SetupState.None)
            {
                throw new Exception("ParseAll is invalid. Current state: " + _globalState);
            }
            _globalState = SetupState.Parsing;
            Log.Info(this, "Script parsing requested.");

            var rtn = new AsyncToken<Void>();
            Async.All(_scriptLoading.ToArray()).OnFinally((_) =>
            {
                Log.Info(this, "Scripts loaded. Starting parsing.");
                
                foreach (var key in _widgetRecords.Keys)
                {
                    ParseWidget(key);
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
                    _globalState = SetupState.Done;
                    
                    rtn.Succeed(Void.Instance);
                });
            });
            return rtn;
        }

        private void ParseWidget(Widget widget)
        {
            var record = _widgetRecords[widget];
            
            if (record.SetupState != SetupState.None)
            {
                return;
            }
            record.SetupState = SetupState.Parsing;

            CreateScripts(widget).OnSuccess(_ =>
            {
                var vineTokens = ParseVines(record);

                if (_globalState == SetupState.Done)
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
                    ParseBehaviors(record,  Async.Map(allToken, __ => Void.Instance));
                }
            });
        }

        public void StartScripts()
        {
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

            // Create scripts
            var scripts = _widgetRecords[widget].Scripts;
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
            }

            // Hook callbacks
            var scriptTokens = new IAsyncToken<Void>[len];
            for (var i = 0; i < len; i++)
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
                
                script.OnUpdated += (_) => Script_OnUpdated(widget, script);
            }
            

            return Async.All(scriptTokens);
        }

        private void Script_OnUpdated(Widget widget, EnkluScript script)
        {
            var record = _widgetRecords[widget];
            
            // Find existing script
            Script existing = null;
            Script newScript;
            
            switch (script.Data.Type)
            {
                case ScriptType.Behavior:
                    for (int i = 0, len = record.Behaviors.Count; i < len; i++)
                    {
                        if (record.Behaviors[i].Data.Id == script.Data.Id)
                        {
                            existing = record.Behaviors[i];
                            break;
                        }
                    }

                    newScript = _scriptFactory.Vine(widget, script);
                    break;
                case ScriptType.Vine:
                    for (int i = 0, len = record.Vines.Count; i < len; i++)
                    {
                        if (record.Vines[i].Data.Id == script.Data.Id)
                        {
                            existing = record.Vines[i];
                            break;
                        }
                    }

                    newScript = _scriptFactory.Behavior(widget, _jsCache, record.Engine, script);
                    break;
                default:
                    throw new Exception("Is there a new script type?!");
            }

            newScript.Configure()
                .OnSuccess(_ =>
                {
                    // TODO: Reload other scripts on this widget too?
                    Log.Info(this, "Swapping script");
                    if (existing != null)
                    {
                        existing.Exit();
                        RemoveScript(record, existing);
                    }
                    
                    AddScript(record, newScript);
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

        /// <summary>
        /// Adds a script to a widget's record.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="script"></param>
        private void AddScript(WidgetRecord record, Script script)
        {
            switch (script.Data.Type)
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
            switch (script.Data.Type)
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
                
                tokens.Add(component.Configure().OnSuccess((_) =>
                {
                    // TODO: Defer this until ScriptService calls
                    StartScript(component);
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

                triggerToken.OnSuccess((_) =>
                {
                    component.Configure();
                    StartScript(component);
                });
            }
        }
    }
}