using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;
using System.Linq;
using Enklu.Orchid;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class BehaviorScript : Script, EnkluScript.IScriptExecutor
    {
        /// <summary>
        /// The JS execution context
        /// </summary>
        private readonly IJsExecutionContext _jsContext;

        /// <summary>
        /// The element this is running on.
        /// </summary>
        private readonly Element _element;

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
        private readonly ElementJs _this;

        /// <inheritdoc />
        public ElementSchema Data
        {
            get { return null == _element ? null : _element.Schema; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BehaviorScript(
            IJsExecutionContext jsContext,
            EnkluScript script,
            Element element,
            ElementJs elementJs)
        {
            if (_isStarted)
            {
                throw new Exception("Script is already running.");
            }

            _element = element;
            _this = elementJs;
            _jsContext = jsContext;

            EnkluScript = script;
            EnkluScript.Executor = this;
        }

        /// <inheritdoc />
        public override IAsyncToken<Void> Configure()
        {
            var token = new AsyncToken<Void>();
            
            try
            {
                Log.Info(this, "Execute : {0}", EnkluScript.Source);
                _jsContext.RunScript(_this, EnkluScript.Program, _module);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not execute script: " + exception);

                token.Fail(exception);
                return token;
            }

            _msgMissing = _module.GetExportedValue<IJsCallback>("msgMissing");
            _enter = _module.GetExportedValue<IJsCallback>("enter");
            _update = _module.GetExportedValue<IJsCallback>("update");
            _exit = _module.GetExportedValue<IJsCallback>("exit");
            IsConfigured = true;
            token.Succeed(Void.Instance);
            return token;
        }

        /// <inheritdoc />
        public override void Enter()
        {
            if (_isStarted)
            {
                throw new Exception("Script already started.");
            }

            Log.Info(this, "Entering script {0} ({1}).", EnkluScript.Data.Name, _element.Name);

            var scriptName = EnkluScript.Data.Name;
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
                    Log.Warning(this, "JavaScript error ({0}) : {1}.", _element.Name, exception);
                }
            }
        }

        /// <inheritdoc />
        public override void FrameUpdate()
        {
            if (_isStarted && null != _update)
            {
                try
                {
                    _update.Apply(_this);
                }
                catch (Exception exception)
                {
                    Log.Warning(this, "JavaScript error ({0}) : {1}.", _element.Name, exception);
                }
            }
        }

        /// <inheritdoc />
        public override void Exit()
        {
            if (!_isStarted)
            {
                throw new Exception("Script hasn't been started.");
            }

            Log.Info(this, "Exiting script {0}.", EnkluScript.Data.Name);

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

        /// <inheritdoc cref="EnkluPlayer.EnkluScript.IScriptExecutor.Send"/>
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