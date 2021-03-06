﻿using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Orchid;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class EnkluScriptElementBehavior : MonoBehaviour, EnkluScript.IScriptExecutor
    {
        /// <summary>
        /// Module id increment
        /// </summary>
        private static int _moduleIds = 1;

        /// <summary>
        /// The JS execution context
        /// </summary>
        private IJsExecutionContext _jsContext;

        /// <summary>
        /// The element this is running on.
        /// </summary>
        private Element _element;

        /// <summary>
        /// Caches JS objects.
        /// </summary>
        private IElementJsCache _jsCache;

        /// <summary>
        /// True iff has been started.
        /// </summary>
        private bool _isStarted;

        /// <summary>
        /// References to JS functions.
        /// </summary>
        private IJsModule _module;
        private IJsCallback _msgMissing;
        private IJsCallback _enter;
        private IJsCallback _update;
        private IJsCallback _exit;

        /// <summary>
        /// A JS reference to this, passed to every ICallable.
        /// </summary>
        private ElementJs _this;

        /// <summary>
        /// Retrieves the <c>EnkluScript</c> instance.
        /// </summary>
        public EnkluScript Script { get; private set; }

        /// <inheritdoc />
        public ElementSchema Data
        {
            get { return null == _element ? null : _element.Schema; }
        }

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="jsCache">Js cache.</param>
        /// <param name="jsContext">JS execution context.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="element">The element.</param>
        public void Initialize(
            IElementJsCache jsCache,
            IJsExecutionContext jsContext,
            EnkluScript script,
            Element element)
        {
            if (_isStarted)
            {
                throw new Exception("Script is already running.");
            }

            _jsContext = jsContext;
            _element = element;
            _jsCache = jsCache;

            Script = script;
            Script.Executor = this;
        }

        /// <summary>
        /// Called after script is ready, before FSM flow.
        /// </summary>
        public void Configure()
        {
            _module = _jsContext.NewModule("module_" + _moduleIds++);
            _this = _jsCache.Element(_element);

            try
            {
                Log.Info(this, "Execute : {0}", Script.Source);
                _jsContext.RunScript(_this, Script.Program, _module);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not execute script: " + exception);

                return;
            }

            _msgMissing = _module.GetExportedValue<IJsCallback>("msgMissing");
            _enter = _module.GetExportedValue<IJsCallback>("enter");
            _update = _module.GetExportedValue<IJsCallback>("update");
            _exit = _module.GetExportedValue<IJsCallback>("exit");
        }

        /// <summary>
        /// Enters the script.
        /// </summary>
        public void Enter()
        {
            if (_isStarted)
            {
                throw new Exception("Script already started.");
            }

            var scriptName = Script.Data.Name;
            var elementName = _element.Schema.GetOwn("name", scriptName).Value;

            Log.Info(this, "Entering script {0} on element: {1}.",
                scriptName, elementName);

            _isStarted = true;

            if (null != _enter)
            {
                try
                {
                    _enter.Apply(_this);
                }
                catch (Exception exception)
                {
                    Log.Warning(this, "JavaScript error : {0}.", exception);
                }
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void FrameUpdate()
        {
            if (_isStarted && null != _update)
            {
                try
                {
                    _update.Apply(_this);
                }
                catch (Exception exception)
                {
                    Log.Warning(this, "JavaScript error : {0}.", exception);
                }
            }
        }

        /// <summary>
        /// Exits the script.
        /// </summary>
        public void Exit()
        {
            if (!_isStarted)
            {
                throw new Exception("Script hasn't been started.");
            }

            Log.Info(this, "Exiting script {0}.", Script.Data.Name);

            _isStarted = false;

            if (null != _exit)
            {
                try
                {
                    _exit.Apply(_this);
                }
                catch (Exception exception)
                {
                    Log.Warning(this, "JavaScript error : {0}.", exception);
                }
            }
        }

        /// <inheritdoc cref="EnkluScript.IScriptExecutor.Send"/>
        public void Send(string name, params object[] parameters)
        {
            var len = parameters.Length;
            var fn = _module.GetExportedValue<IJsCallback>(name);
            if (null != fn)
            {
                if (len == 0)
                {
                    fn.Apply(_this);
                }
                else
                {
                    try
                    {
                        fn.Apply(_this, parameters);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                }
            }
            else if (null != _msgMissing)
            {
                if (parameters.Length == 0)
                {
                    try
                    {
                        _msgMissing.Apply(_this, name);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                }
                else
                {
                    var values = new object[] {name}.Concat(parameters).ToArray();

                    try
                    {
                        _msgMissing.Apply(_this, values);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                }
            }
        }
    }
}