using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint.Parser.Ast;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptRunner
    {
        public enum RunnerState
        {
            None,
            Starting,
            Running,
            Stopping
        }
        
        public enum SetupState
        {
            None,
            Loading,
            Ready
        }

        private class WidgetRecord
        {
            public Widget Widget;

            public SetupState SetupState;
            public IScriptAssembler Assembler;

            public VineScript[] Vines;
            public BehaviorScript[] Behaviors;

            public WidgetRecord ParentRecord;
            public List<WidgetRecord> DescendentRecords;

            public Action<Script[], Script[]> OnScriptsUpdated;
            public IAsyncToken<Void> BuildScripts()
            {
                if (SetupState != SetupState.None)
                {
                    throw new Exception("Widget's scripts are already loading.");
                }
                SetupState = SetupState.Loading;
                
                var rtnToken = new AsyncToken<Void>();

                Assembler.OnScriptsUpdated += (old, @new) =>
                {
                    switch (SetupState)
                    {
                        // First time this Widget's scripts are ready.
                        case SetupState.Loading:
                            var descendentTokens = new IAsyncToken<Void>[DescendentRecords.Count];
                            
                            for (var i = 0; i < DescendentRecords.Count; i++)
                            {
                                var subRecord = DescendentRecords[i];
                                descendentTokens[i] = subRecord.BuildScripts();
                            }

                            Async.All(descendentTokens).OnFinally(_ => rtnToken.Succeed(Void.Instance));

                            SetupState = SetupState.Ready;
                            break;
                        // Update to scripts
                        case SetupState.Ready:

                            break;
                    }
                    
                    OnScriptsUpdated.Execute(old, @new);
                };
                Assembler.Setup(Widget);

                return rtnToken;
            }

            public void RunScripts(ScriptType type)
            {
                
            }
        }

        /// <summary>
        /// A mini factory for creating ScriptAssemblers.
        /// </summary>
        private readonly Func<IScriptAssembler> _createScriptAssembler;

        private WidgetRecord _rootRecord;

        private RunnerState _runnerState;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptRunner(
            IScriptManager scriptManager, 
            IScriptFactory scriptFactory, 
            IElementJsCache jsCache,
            IScriptRequireResolver requireResolver,
            AppJsApi appJsApi)
        {
            _createScriptAssembler = () => new ScriptAssembler(
                scriptManager,
                scriptFactory,
                jsCache,
                requireResolver,
                appJsApi);
        }

        public void AddSceneRoot(Widget root)
        {
            _rootRecord = CreateRecord(root);
            _rootRecord.BuildScripts();
        }

        private WidgetRecord CreateRecord(Widget widget)
        {
            var record = new WidgetRecord
            {
                Widget = widget,
                SetupState = SetupState.Loading,
                Assembler = _createScriptAssembler()
            };

            record.Assembler.OnScriptsUpdated += (old, @new) => OnWidgetUpdated(record, old, @new);

            // Populate descendent records.
            var descendents = new List<Widget>();
            FindWidgetDescendents(widget, descendents);

            var descendentRecords = new List<WidgetRecord>(widget.Children.Count); 
            
            for (int i = 0, len = descendents.Count; i < len; i++)
            {
                var descendentRecord = CreateRecord(descendents[i]);
                descendentRecord.ParentRecord = record;
                descendentRecords.Add(descendentRecord);
            }
            record.DescendentRecords = descendentRecords;

            return record;
        }

        private void OnWidgetUpdated(WidgetRecord record, Script[] old, Script[] @new)
        {
            
        }

        /// <summary>
        /// Find all immediate Widget descendents of a Widget. If a child isn't a Widget then its
        /// children are searched recursively until a Widget is found.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="container"></param>
        private void FindWidgetDescendents(Element element, List<Widget> container)
        {
            if (element.Children.Count == 0)
            {
                return;
            }
            
            /*    Traverse children recursively looking for Widgets, ignoring children of Widgets.
             
                R            R  - Root Widget
               / \           W  - Widget, added to container
              /   \          W* - Widget, not included since it has a parent Widget
             W     E         E  - Element, its children are traversed to potentially be added.
             |     | \
             |     |  \
             W*    W   W
             
            */

            for (int i = 0, len = element.Children.Count; i < len; i++)
            {
                var childWidget = element.Children[i] as Widget;
                if (childWidget != null)
                {
                    container.Add(childWidget);
                }
                else
                {
                    FindWidgetDescendents(element.Children[i], container);
                }
            }
        }
        
        

//        public IAsyncToken<Void> ParseAll()
//        {
//            if (_runnerState != RunnerState.None)
//            {
//                throw new Exception("ParseAll is invalid. Already setup.");
//            }
//            Log.Warning(this, "Script parsing requested.");
//
//            var rtnToken = new AsyncToken<Void>();
//            Exception exception = null;
//            Async.All(_scriptLoading.ToArray()).OnFinally((_) =>
//            {
//                Log.Warning(this, "Scripts loaded. Starting parsing.");
//
//                for (int i = 0, len = _widgetRecords.Count; i < len; i++)
//                {
//                    ParseWidget(_widgetRecords[i]);
//                }
//
//                Async.All(_vineTokens.ToArray()).OnSuccess((__) =>
//                {
//                    Log.Warning(this, "Vines parsed. Parsing Behaviors.");
//                    
//                    // Currently, this triggers behavior parsing/entering
//                    _vinesComplete.Succeed(Void.Instance);
//
//                    for (int i = 0, len = _widgetRecords.Count; i < len; i++)
//                    {
//                        // TODO: Don't blindly assume success
//                        _widgetRecords[i].SetupState = SetupState.Done;
//                    }
//
//                    Log.Warning(this, "All parsing complete.");
//
//                    exception = null;
//                })
//                .OnFinally(__ =>
//                {
//                    // Delay resolving rtnToken until finally, to ensure the proper
//                    // RunnerState is set before external calls may try to interact with ScriptRunner again.
//                    _runnerState = RunnerState.Idle;
//                    if (exception == null)
//                    {
//                        rtnToken.Succeed(Void.Instance);
//                    }
//                    else
//                    {
//                        rtnToken.Fail(exception);
//                    }
//                });
//            });
//            return rtnToken;
//        }

//        private IAsyncToken<Void> ParseWidget(WidgetRecord record)
//        {   
//            if (record.SetupState != SetupState.Loaded)
//            {
//                throw new Exception("Should this be an exception? " + record.SetupState);
//            }
//            record.SetupState = SetupState.Parsing;
//
//            
//            var rtnToken = record.LoadToken.OnSuccess(_ =>
//            {
//                var vineTokens = ParseVines(record);
//
//                if (_runnerState == RunnerState.None)
//                {
//                    for (int i = 0, len = vineTokens.Count; i < len; i++)
//                    {
//                        _vineTokens.Add(vineTokens[i]);
//                    }
//
//                    ExecuteBehaviors(record, _vinesComplete);
//                }
//                else
//                {
//                    var allToken = Async.All(vineTokens.ToArray());
//                    ExecuteBehaviors(record, Async.Map(allToken, __ => Void.Instance));
//                }
//            });
//
//            return Async.Map(rtnToken, _ => Void.Instance);
//        }

        /// <summary>
        /// Start all scripts.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void StartAllScripts()
        {
            Log.Warning(this, "StartAllScripts");

            if (_runnerState != RunnerState.None)
            {
                throw new Exception(string.Format("ScriptRunner is in an invalid state ({0})", _runnerState));
            }

            _rootRecord.BuildScripts()
                .OnSuccess(_ => { _runnerState = RunnerState.Running; })
                .OnFailure(exception => { Log.Error(this, "Error starting scripts: " + exception); });
        }

        /// <summary>
        /// Stop all scripts.
        /// </summary>
        /// <exception cref="Exception"></exception>
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
                var vines = _widgetRecords[i].Vines;
                
                for (int j = 0, jLen = vines.Length; j < jLen; j++)
                {
                    StopScript(vines[i]);
                }
            }

            for (var i = 0; i < len; i++)
            {
                var behaviors = _widgetRecords[i].Behaviors;
                
                for (int j = 0, jLen = behaviors.Length; j < jLen; j++)
                {
                    StopScript(behaviors[i]);
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

                for (int j = 0, jLen = behaviorComponents.Length; j < jLen; j++)
                {
                    behaviorComponents[j].FrameUpdate();
                }
            }
        } 
        
        /// <summary>
        /// Starts all scripts on the given record, and its descendent records.
        /// </summary>
        /// <param name="record"></param>
        private void StartRecord(WidgetRecord record)
        {
            for (int i = 0, len = record.Vines.Length; i < len; i++)
            {
                StartScript(record.Vines[i]);
            }
            
            for (int i = 0, len = record.Behaviors.Length; i < len; i++)
            {
                StartScript(record.Behaviors[i]);
            }
            
            for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
            {
                StartRecord(record.DescendentRecords[i]);
            }
        }
        
        /// <summary>
        /// Stops all scripts on the given record, and its descendent records.
        /// </summary>
        /// <param name="record"></param>
        private void StopRecord(WidgetRecord record)
        {
            for (int i = 0, len = record.Vines.Length; i < len; i++)
            {
                StopScript(record.Vines[i]);
            }
            
            for (int i = 0, len = record.Behaviors.Length; i < len; i++)
            {
                StopScript(record.Behaviors[i]);
            }
            
            for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
            {
                StopRecord(record.DescendentRecords[i]);
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
        
//        private List<ContentWidget> waitingWidgets = new List<ContentWidget>();
//
//        private void LoadScripts(WidgetRecord record)
//        {
//            try
//            {
//                var scriptToken = CreateScripts(record);
//                
//                var contentWidget = record.Widget as ContentWidget;
//                if (contentWidget != null)
//                {
//                    waitingWidgets.Add(contentWidget);
//                    record.LoadToken = Async.All(
//                    new [] { Async.Map(scriptToken, _ => Void.Instance) }, 
//                    new [] { Async.Map(contentWidget.OnAssetLoaded, _ =>
//                    {
//                        waitingWidgets.Remove(_);
//                        Log.Warning(this, "Waiting widgets: " + waitingWidgets.Count + " " + waitingWidgets[0]);
//                        return Void.Instance;
//                    }) });
//                }
//                else
//                {
//                    record.LoadToken = scriptToken;
//                }
//
//                if (record.LoadToken == null)
//                {
//                    Log.Error(this, "Null load token?!?!?!" + record.Widget);
//                }
//            }
//            catch (Exception e)
//            {
//                Log.Error("Error running scripts for {0} : {1}", record.Widget, e);
//                record.SetupState = SetupState.Error;
//            }
//        }

//        /// <summary>
//        /// Sets up Vine instances for a given WidgetRecord. Vine configuration tokens are returned.
//        /// </summary>
//        /// <param name="record"></param>
//        /// <returns></returns>
//        private List<IAsyncToken<Void>> ParseVines(WidgetRecord record)
//        {
//            List<IAsyncToken<Void>> tokens = new List<IAsyncToken<Void>>();
//
//            for (int i = 0, len = record.Scripts.Count; i < len; i++)
//            {
//                var script = record.Scripts[i];
//                if (script.Data.Type != ScriptType.Vine)
//                {
//                    continue;
//                }
//
//                Log.Info(this, "ParseVines");
//                var component = _scriptFactory.Vine(record.Widget, script);
//                AddScript(record, component);
//                
//                tokens.Add(component.Configure().OnSuccess((_) =>
//                {
//                    // TODO: Defer this until ScriptService calls
//                    if (_runnerState == RunnerState.Running)
//                    {
////                        StopScript(component);
//                        StartScript(component);
//                    }
//                }));
//            }
//
//            return tokens; 
//        }
//
//        /// <summary>
//        /// Sets up Behavior instances for a given WidgetRecord.
//        /// All configuration happens on success of the trigger token.
//        /// </summary>
//        /// <param name="record"></param>
//        /// <param name="triggerToken"></param>
//        private void ExecuteBehaviors(WidgetRecord record, IAsyncToken<Void> triggerToken)
//        {
//            for (int i = 0, len = record.Scripts.Count; i < len; i++)
//            {
//                var script = record.Scripts[i];
//                if (script.Data.Type != ScriptType.Behavior)
//                {
//                    continue;
//                }
//
//                var component = _scriptFactory.Behavior(record.Widget, _jsCache, record.Engine, script);
//                AddScript(record, component);
//                
//                triggerToken.OnSuccess((_) =>
//                {
//                    component.Configure();
//                    if (_runnerState == RunnerState.Running)
//                    {
//                        StopScript(component);
//                        StartScript(component);
//                    }
//                });
//            }
//        }
    }
}