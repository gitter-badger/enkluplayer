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
            
            var scripts = record.Scripts;
                
            for (int i = 0, len = scripts.Count; i < len; i++)
            {
                switch (scripts[i].Data.Type)
                {
                    case ScriptType.Vine:
                    {
                        var vineComponent = _scriptFactory.Vine(widget, scripts[i]);
                        
                        record.Vines.Add(vineComponent);
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
                            widget, _jsCache, record.Engine, scripts[i]);
                            
                        record.Behaviors.Add(behaviorComponent);
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
                
                // TODO: Support script updating
//                script.OnUpdated += Script_OnUpdated;
            }
            

            return Async.All(scriptTokens);
        }

        private void Script_OnUpdated(WidgetRecord record, EnkluScript script)
        {
            // Find existing script
            Script existing;
            
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
                    break;
            }
            
            
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