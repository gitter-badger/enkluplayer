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
    /// Handles running all scripts for a given scene root and its descendent Elements.
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
        /// The state a ElementRecord is in.
        /// </summary>
        public enum RecordState
        {
            None,
            Loading,
            Ready
        }

        /// <summary>
        /// Tracks an Element's scripts and their current state with the runner.
        /// </summary>
        private class ElementRecord
        {
            /// <summary>
            /// The Element.
            /// </summary>
            public Element Element;

            /// <summary>
            /// The state of the record.
            /// </summary>
            public RecordState RecordState;
            
            /// <summary>
            /// The IScriptAssembler used for this Element.
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
            public ElementRecord ParentRecord;
            
            /// <summary>
            /// The records below this record in the hierarchy.
            /// </summary>
            public List<ElementRecord> ChildRecords;

            /// <summary>
            /// Invoked when scripts have updated, with the old then new as payloads.
            /// </summary>
            public event Action<Script[], Script[]> OnScriptsUpdated;

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
                    if (RecordState == RecordState.Loading)
                    {
                        // First time this Element's scripts are ready.
                        var childTokens = new IAsyncToken<Void>[ChildRecords.Count];
                            
                        for (var i = 0; i < ChildRecords.Count; i++)
                        {
                            var subRecord = ChildRecords[i];
                            childTokens[i] = subRecord.LoadScripts();
                        }

                        Async.All(childTokens).OnFinally(_ => _loadToken.Succeed(Void.Instance));

                        RecordState = RecordState.Ready;
                    }
                    
                    OnScriptsUpdated.Execute(old, @new);
                };
                Assembler.Setup(Element);

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
        private ElementRecord _rootRecord;

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
            IScriptExecutorFactory scriptExecutorFactory,
            IElementJsCache jsCache,
            AppJsApi appJsApi)
        {
            _createScriptAssembler = () => new ScriptAssembler(
                scriptManager, 
                scriptFactory, 
                scriptExecutorFactory,
                jsCache,
                appJsApi);
        }

        /// <summary>
        /// Adds an Element as the root element.
        /// </summary>
        public void AddSceneRoot(Element root)
        {
            _rootRecord = CreateRecord(root);
            _rootRecord.LoadScripts();
        }
        
        /// <summary>
        /// Start all scripts.
        /// </summary>
        public IAsyncToken<Void> StartRunner()
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
                    Log.Warning(this, "All scripts loaded. Configuring Vines.");
                    
                    var vineTokens = new List<IAsyncToken<Void>>();
                    var behaviorTokens = new List<IAsyncToken<Void>>();
                    
                    ConfigureRecord(_rootRecord, ScriptType.Vine, vineTokens);

                    Async.All(vineTokens.ToArray())
                        .OnFinally(__ =>
                        {
                            Log.Warning(this, "All Vines ({0}) configured. Configuring Behaviors.", vineTokens.Count);
                            ConfigureRecord(_rootRecord, ScriptType.Behavior, behaviorTokens);
    
                            Async.All(behaviorTokens.ToArray())
                                .OnFinally(___ =>
                                {
                                    Log.Warning(this, "All Behaviors ({0}) configured. Starting Scripts", behaviorTokens.Count);
                                    
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
        public void StopRunner()
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
        /// Creates an ElementRecord and all of its descendent records.
        /// </summary>
        private ElementRecord CreateRecord(Element element)
        {
            var record = new ElementRecord
            {
                Element = element,
                RecordState = RecordState.None,
                Assembler = _createScriptAssembler()
            };

            record.Assembler.OnScriptsUpdated += (old, @new) => Element_OnScriptsUpdated(record, old, @new);

            var childCount = element.Children.Count;
            var childrenRecords = new List<ElementRecord>(element.Children.Count); 
            
            for (var i = 0; i < childCount; i++)
            {
                var child = element.Children[i];
                var childRecord = CreateRecord(child);
                childRecord.ParentRecord = record;
                
                childrenRecords.Add(childRecord);
            }
            record.ChildRecords = childrenRecords;

            element.OnChildAdded += Element_OnChildAdded;
            element.OnChildRemoved += Element_OnChildRemoved;

            return record;
        }

        /// <summary>
        /// Configures a ElementRecord's scripts.
        /// </summary>
        /// <param name="record">The record to configure.</param>
        /// <param name="type">The Type of script to configure.</param>
        /// <param name="tokenContainer">The container configuration tokens will be put in.</param>
        /// <param name="recursive">Whether or not this configuration should affect descendent records or not.</param>
        private static void ConfigureRecord(ElementRecord record, ScriptType type, List<IAsyncToken<Void>> tokenContainer, bool recursive = true)
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
                for (int i = 0, len = record.ChildRecords.Count; i < len; i++)
                {
                    ConfigureRecord(record.ChildRecords[i], type, tokenContainer);
                }
            }
        }

        /// <summary>
        /// Enters scripts on a record and optionally its descendents as well.
        /// </summary>
        /// <param name="record">The record to start.</param>
        /// <param name="type">The type of scripts to start.</param>
        /// <param name="recursive">Whether to affect descendent records or not.</param>
        private void StartRecord(ElementRecord record, ScriptType type, bool recursive = true)
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
                            Log.Error(this, "Error entering script ({0} : {1})", vine, e);
                        }
                    }
                }
                
                if (recursive)
                {
                    for (int i = 0, len = record.ChildRecords.Count; i < len; i++)
                    {
                        StartRecord(record.ChildRecords[i], ScriptType.Vine);
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
                    for (int i = 0, len = record.ChildRecords.Count; i < len; i++)
                    {
                        StartRecord(record.ChildRecords[i], ScriptType.Behavior);
                    }
                }
            }
        }
        
        /// <summary>
        /// Stops all scripts on the given record, and its descendent records.
        /// </summary>
        /// <param name="record">The record to stop.</param>
        /// <param name="recursive">Whether descendent records should be affected or not.</param>
        private void StopRecord(ElementRecord record, bool recursive = true)
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

            if (recursive)
            {
                for (int i = 0, len = record.ChildRecords.Count; i < len; i++)
                {
                    StopRecord(record.ChildRecords[i]);
                }
            }
        }

        /// <summary>
        /// Updates a record, and all of its descendents.
        /// </summary>
        /// <param name="record"></param>
        private void UpdateRecord(ElementRecord record)
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

            for (int i = 0, len = record.ChildRecords.Count; i < len; i++)
            {
                UpdateRecord(record.ChildRecords[i]);
            }
        }

        /// <summary>
        /// Recursively finds an ElementRecord for a given Element, using an optional starting record.
        /// Returns null if no record is found.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="searchStart"></param>
        /// <returns></returns>
        private ElementRecord FindRecord(Element element, ElementRecord searchStart = null)
        {
            if (searchStart == null)
            {
                searchStart = _rootRecord;
            }
            
            if (searchStart.Element == element)
            {
                return searchStart;
            }

            for (int i = 0, len = searchStart.ChildRecords.Count; i < len; i++)
            {
                var record = FindRecord(element, searchStart.ChildRecords[i]);
                if (record != null)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>
        /// Invoked when a record's scripts have updated. This can be called before a record has been configured.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="old"></param>
        /// <param name="new"></param>
        private void Element_OnScriptsUpdated(ElementRecord record, Script[] old, Script[] @new)
        {
            if (record.RecordState == RecordState.Ready && _runnerState == RunnerState.Running)
            {
                StopRecord(record, false);
            }
            
            
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

            record.Vines = vines.ToArray();
            record.Behaviors = behaviors.ToArray();

            if (record.RecordState == RecordState.Ready && _runnerState == RunnerState.Running)
            {
                StartRecord(record, ScriptType.Vine, false);
                StartRecord(record, ScriptType.Behavior, false);
            }
        }

        /// <summary>
        /// Invoked when an Element has been removed from its parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prevChild"></param>
        /// <exception cref="Exception"></exception>
        private void Element_OnChildRemoved(Element parent, Element prevChild)
        {
            Log.Warning(this, "OnChildRemoved");
            var parentRecord = FindRecord(parent);
            ElementRecord prevChildRecord = null;

            for (int i = 0, len = parentRecord.ChildRecords.Count; i < len; i++)
            {
                var childRecord = parentRecord.ChildRecords[i];
                if (childRecord.Element == prevChild)
                {
                    prevChildRecord = childRecord;
                    break;
                }
            }

            if (prevChildRecord == null)
            {
                throw new Exception("No record found for removed child.");
            }

            StopRecord(prevChildRecord);
            parentRecord.ChildRecords.Remove(prevChildRecord);
        }

        /// <summary>
        /// Called when an Element gets a new child.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        private void Element_OnChildAdded(Element parent, Element child)
        {
            // Ignore if the runner isn't running. This will proc as elements in the initial scene graph get built up.
            if (_runnerState == RunnerState.Running)
            {
                var parentRecord = FindRecord(parent);
                var childRecord = CreateRecord(child);
                childRecord.LoadScripts().OnSuccess(_ =>
                {
                    var vineTokens = new List<IAsyncToken<Void>>();
                    ConfigureRecord(childRecord, ScriptType.Vine, vineTokens);

                    Async.All(vineTokens.ToArray()).OnSuccess(__ =>
                    {
                        Log.Warning(this, "Vines");
                        var behaviorTokens = new List<IAsyncToken<Void>>();
                        ConfigureRecord(childRecord, ScriptType.Behavior, behaviorTokens);

                        Async.All(behaviorTokens.ToArray()).OnSuccess(___ =>
                        {
                            Log.Warning(this, "Behaviours");
                            parentRecord.ChildRecords.Add(childRecord);
                        
                            StartRecord(childRecord, ScriptType.Vine);
                            StartRecord(childRecord, ScriptType.Behavior);
                        });
                    });
                });
            }
        }
    }
}