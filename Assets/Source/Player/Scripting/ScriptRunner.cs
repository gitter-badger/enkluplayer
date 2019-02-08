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
    /// <summary>
    /// Handles running all scripts for a given scene root and its descendent Widgets.
    /// Script loading and running is centralized and deterministic.
    /// </summary>
    public class ScriptRunner
    {
        /// <summary>
        /// The state the ScriptRunner is in.
        /// </summary>
        public enum RunnerState
        {
            /// <summary>
            /// Nothing. The default state, or after being stopped.
            /// </summary>
            None,
            
            /// <summary>
            /// Currently waiting for scripts to load and/or configure.
            /// </summary>
            Starting,
            
            /// <summary>
            /// All known scripts are loaded, configured, and can be safely run.
            /// </summary>
            Running,
            
            /// <summary>
            /// Stopping all scripts.
            /// </summary>
            Stopping
        }
        
        /// <summary>
        /// The state a WidgetRecord is in.
        /// </summary>
        public enum RecordState
        {
            None,
            Loading,
            Ready
        }

        /// <summary>
        /// Tracks a Widget's scripts and their current state with the runner.
        /// </summary>
        private class WidgetRecord
        {
            /// <summary>
            /// The widget.
            /// </summary>
            public Widget Widget;

            /// <summary>
            /// The state of the record.
            /// </summary>
            public RecordState RecordState;
            
            /// <summary>
            /// The IScriptAssembler used for this widget.
            /// </summary>
            public IScriptAssembler Assembler;

            /// <summary>
            /// Current VineScript instances.
            /// </summary>
            public VineScript[] Vines;
            
            /// <summary>
            /// Current Behavior instances.
            /// </summary>
            public BehaviorScript[] Behaviors;

            /// <summary>
            /// The record above this record.
            /// </summary>
            public WidgetRecord ParentRecord;
            
            /// <summary>
            /// The records below this record in the hierarchy.
            /// </summary>
            public List<WidgetRecord> DescendentRecords;

            /// <summary>
            /// Invoked when scripts have updated, with the old then new as payloads.
            /// </summary>
            public Action<Script[], Script[]> OnScriptsUpdated;

            /// <summary>
            /// Cached load token.
            /// </summary>
            private AsyncToken<Void> _loadToken = new AsyncToken<Void>();
            
            /// <summary>
            /// Starts loading scripts if they haven't already been loaded for this record and all descendent records.
            /// </summary>
            /// <returns></returns>
            public IAsyncToken<Void> LoadScripts()
            {
                if (RecordState != RecordState.None)
                {
                    return _loadToken;
                }
                RecordState = RecordState.Loading;

                Assembler.OnScriptsUpdated += (old, @new) =>
                {
                    switch (RecordState)
                    {
                        // First time this Widget's scripts are ready.
                        case RecordState.Loading:

                            var vines = new List<VineScript>();
                            var behaviors = new List<BehaviorScript>();
                            for (var i = 0; i < @new.Length; i++)
                            {
                                var script = @new[i];
                                switch (script.EnkluScript.Data.Type)
                                {
                                    case ScriptType.Vine:
                                        vines.Add((VineScript) script);
                                        break;
                                    case ScriptType.Behavior:
                                        behaviors.Add((BehaviorScript) script);
                                        break;
                                    default:
                                        Log.Error(this, "Unknown Script.");
                                        break;
                                }
                            }

                            Vines = vines.ToArray();
                            Behaviors = behaviors.ToArray();
                            
                            var descendentTokens = new IAsyncToken<Void>[DescendentRecords.Count];
                            
                            for (var i = 0; i < DescendentRecords.Count; i++)
                            {
                                var subRecord = DescendentRecords[i];
                                descendentTokens[i] = subRecord.LoadScripts();
                            }

                            Async.All(descendentTokens).OnFinally(_ => _loadToken.Succeed(Void.Instance));

                            RecordState = RecordState.Ready;
                            break;
                        // Update to scripts
                        case RecordState.Ready:
                            
                            break;
                    }
                    
                    OnScriptsUpdated.Execute(old, @new);
                };
                Assembler.Setup(Widget);

                return _loadToken;
            }
        }

        /// <summary>
        /// A mini factory for creating ScriptAssemblers.
        /// </summary>
        private readonly Func<IScriptAssembler> _createScriptAssembler;

        /// <summary>
        /// The scene's root record.
        /// </summary>
        private WidgetRecord _rootRecord;

        /// <summary>
        /// The state of the runner.
        /// </summary>
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

        /// <summary>
        /// Adds a Widget as the root element.
        /// </summary>
        public void AddSceneRoot(Widget root)
        {
            _rootRecord = CreateRecord(root);
            _rootRecord.LoadScripts();
        }
        
        /// <summary>
        /// Start all scripts.
        /// </summary>
        public IAsyncToken<Void> StartAllScripts()
        {
            Log.Warning(this, "StartAllScripts");

            if (_runnerState != RunnerState.None)
            {
                throw new Exception(string.Format("ScriptRunner is in an invalid state. ({0})", _runnerState));
            }

            _runnerState = RunnerState.Starting;

            return _rootRecord.LoadScripts()
                .OnSuccess(_ =>
                {
                    Log.Info(this, "All scripts loaded. Configuring Vines.");
                    
                    var vineTokens = new List<IAsyncToken<Void>>();
                    var behaviorTokens = new List<IAsyncToken<Void>>();
                    
                    ConfigureRecord(_rootRecord, ScriptType.Vine, vineTokens);

                    Async.All(vineTokens.ToArray())
                        .OnFinally(__ =>
                        {
                            Log.Info(this, "All Vines ({0}) configured. Configuring Behaviors.", vineTokens.Count);
                            ConfigureRecord(_rootRecord, ScriptType.Behavior, behaviorTokens);
    
                            Async.All(behaviorTokens.ToArray())
                                .OnFinally(___ =>
                                {
                                    Log.Info(this, "All Behaviors ({0}) configured. Starting Scripts", behaviorTokens.Count);
                                    
                                    if (_runnerState != RunnerState.Starting)
                                    {
                                        Log.Warning(this, "Scripts stopped before building finished.");
                                        return;
                                    }
        
                                    StartRecord(_rootRecord, ScriptType.Vine);
                                    StartRecord(_rootRecord, ScriptType.Behavior);
        
                                    _runnerState = RunnerState.Running;
                                });
                        });
                })
                .OnFailure(exception => { Log.Error(this, "Error starting scripts: " + exception); });
        }
        
        /// <summary>
        /// Stop all scripts.
        /// </summary>
        public void StopAllScripts()
        {
            Log.Warning(this, "StopAllScripts");
            
            if (_runnerState != RunnerState.Running)
            {
                throw new Exception(string.Format("Scripts weren't running. ({0})", _runnerState));
            }
            _runnerState = RunnerState.Stopping;
            
            StopRecord(_rootRecord);
        }
        
        /// <summary>
        /// Updates scripts if the runner is running.
        /// </summary>
        public void Update()
        {
            if (_runnerState != RunnerState.Running)
            {
                return;
            }
            
            // Vines currently have no Update usage, so skip for now.

            UpdateRecord(_rootRecord);
        }

        /// <summary>
        /// Creates a WidgetRecord and all of its descendent records.
        /// </summary>
        private WidgetRecord CreateRecord(Widget widget)
        {
            var record = new WidgetRecord
            {
                Widget = widget,
                RecordState = RecordState.None,
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

        /// <summary>
        /// Configures a WidgetRecord's scripts.
        /// </summary>
        /// <param name="record">The record to configure.</param>
        /// <param name="type">The Type of script to configure.</param>
        /// <param name="tokenContainer">The container configuration tokens will be put in.</param>
        /// <param name="recursive">Whether or not this configuration should affect descendent records or not.</param>
        private static void ConfigureRecord(WidgetRecord record, ScriptType type, List<IAsyncToken<Void>> tokenContainer, bool recursive = true)
        {
            switch (type)
            {
                case ScriptType.Vine:
                    if (record.Vines != null)
                    {
                        for (var i = 0; i < record.Vines.Length; i++)
                        {
                            tokenContainer.Add(record.Vines[i].Configure());
                        }
                    }
                    break;
                
                case ScriptType.Behavior:
                    if (record.Behaviors != null)
                    {
                        for (var i = 0; i < record.Behaviors.Length; i++)
                        {
                            tokenContainer.Add(record.Behaviors[i].Configure());
                        }
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown ScriptType.");
            }

            if (recursive)
            {
                for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
                {
                    ConfigureRecord(record.DescendentRecords[i], type, tokenContainer, recursive);
                }
            }
        }

        /// <summary>
        /// Enters scripts on a record and optionally its descendents as well.
        /// </summary>
        /// <param name="record">The record to start.</param>
        /// <param name="type">The type of scripts to start.</param>
        /// <param name="recursive">Whether to affect descendent records or not.</param>
        private void StartRecord(WidgetRecord record, ScriptType type, bool recursive = true)
        {
            // TODO: Make the ScriptType parameter a bitmask so both can be run together?
            
            if (type == ScriptType.Vine)
            {
                if (record.Vines != null)
                {
                    for (int i = 0, len = record.Vines.Length; i < len; i++)
                    {
                        var vine = record.Vines[i];
                        try
                        {
                            Log.Debug(this, "Entering script ({0})", vine);
                            vine.Enter();
                        }
                        catch (Exception e)
                        {
                            Log.Error(this, "Error entering script ({0})", vine);
                        }
                    }
                }
                
                if (recursive)
                {
                    for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
                    {
                        StartRecord(record.DescendentRecords[i], ScriptType.Vine);
                    }
                }
            }

            if (type == ScriptType.Behavior)
            {
                if (record.Behaviors != null)
                {
                    for (int i = 0, len = record.Behaviors.Length; i < len; i++)
                    {
                        var behavior = record.Behaviors[i];
                        try
                        {
                            Log.Debug(this, "Entering script ({0})", behavior);
                            behavior.Enter();
                        }
                        catch (Exception e)
                        {
                            Log.Error(this, "Error entering script ({0}): {1}", behavior, e);
                        }
                    }
                }

                if (recursive)
                {
                    for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
                    {
                        StartRecord(record.DescendentRecords[i], ScriptType.Behavior);
                    }
                }
            }
        }
        
        /// <summary>
        /// Stops all scripts on the given record, and its descendent records.
        /// </summary>
        /// <param name="record">The record to stop.</param>
        /// <param name="recursive">Whether descendent records should be affected or not.</param>
        private void StopRecord(WidgetRecord record, bool recursive = true)
        {
            if (record.Vines != null)
            {
                for (int i = 0, len = record.Vines.Length; i < len; i++)
                {
                    try
                    {
                        record.Vines[i].Exit();
                    }
                    catch (Exception e)
                    {
                        Log.Error(this, "Error exiting script ({0}): {1}", record.Vines[i], e);
                    }
                }
            }

            if (record.Behaviors != null)
            {
                for (int i = 0, len = record.Behaviors.Length; i < len; i++)
                {
                    try
                    {
                        record.Behaviors[i].Exit();
                    }
                    catch (Exception e)
                    {
                        Log.Error(this, "Error exiting script ({0}): {1}", record.Behaviors[i], e);
                    }
                }
            }

            if (!recursive)
            {
                for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
                {
                    StopRecord(record.DescendentRecords[i]);
                }
            }
        }

        /// <summary>
        /// Updates a record, and all of its descendents.
        /// </summary>
        /// <param name="record"></param>
        private void UpdateRecord(WidgetRecord record)
        {
            // Vines don't have an update step. No need to worry for now!
            for (int i = 0, len = record.Behaviors.Length; i < len; i++)
            {
                try
                {
                    record.Behaviors[i].FrameUpdate();
                }
                catch (Exception e)
                {
                    // Should this actually spam errors?!
                    Log.Error(this, "Error updating script ({0}): {1}", record.Behaviors[i], e);
                }
            }

            for (int i = 0, len = record.DescendentRecords.Count; i < len; i++)
            {
                UpdateRecord(record.DescendentRecords[i]);
            }
        }

        /// <summary>
        /// Invoked when a record's scripts have updated.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="old"></param>
        /// <param name="new"></param>
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
    }
}