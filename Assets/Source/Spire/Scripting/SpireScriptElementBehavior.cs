using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;
using Jint.Runtime;
using UnityEngine;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class SpireScriptElementBehavior : MonoBehaviour, SpireScript.IScriptExecutor
    {
        /// <summary>
        /// An engine to run the scripts with.
        /// </summary>
        private Engine _engine;

        /// <summary>
        /// The element this is running on.
        /// </summary>
        private Element _element;

        /// <summary>
        /// Creates ElementJs instances.
        /// </summary>
        private IElementJsFactory _factory;

        /// <summary>
        /// True iff has been started.
        /// </summary>
        private bool _isStarted;

        /// <summary>
        /// References to JS functions.
        /// </summary>
        private ICallable _msgMissing;
        private ICallable _enter;
        private ICallable _update;
        private ICallable _exit;

        /// <summary>
        /// A JS reference to this, passed to every ICallable.
        /// </summary>
        private JsValue _this;

        /// <summary>
        /// Arguments.
        /// </summary>
        private readonly JsValue[] _nullArgs = new JsValue[0];

        /// <summary>
        /// Retrieves the <c>SpireScript</c> instance.
        /// </summary>
        public SpireScript Script { get; private set; }

        /// <inheritdoc />
        public ElementSchema Data
        {
            get { return null == _element ? null : _element.Schema; }
        }

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="factory">Creates elements.</param>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="element">The element.</param>
        public void Initialize(
            IElementJsFactory factory,
            Engine engine,
            SpireScript script,
            Element element)
        {
            if (_isStarted)
            {
                throw new Exception("Script is already running.");
            }

            _engine = engine;
            _element = element;
            _factory = factory;

            Script = script;
            Script.Executor = this;
        }

        /// <summary>
        /// Called after script is ready, before FSM flow.
        /// </summary>
        public void Configure()
        {
            var thisBinding = JsValue.FromObject(
                _engine,
                _factory.Instance(_engine, _element));
            _engine.ExecutionContext.ThisBinding = thisBinding;

            try
            {
                Log.Info(this, "Execute : {0}", Script.Source);
                _engine.Execute(Script.Program);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not execute script: " + exception);

                return;
            }

            _this = thisBinding;

            _msgMissing = _engine.GetFunction("msgMissing");
            _enter = _engine.GetFunction("enter");
            _update = _engine.GetFunction("update");
            _exit = _engine.GetFunction("exit");
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

            Log.Info(this, "Entering script {0}.", Script.Data.Name);

            _isStarted = true;

            if (null != _enter)
            {
                try
                {
                    _enter.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
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
                    _update.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
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
                    _exit.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
                {
                    Log.Warning(this, "JavaScript error : {0}.", exception);
                }
            }
        }

        /// <inheritdoc cref="SpireScript.IScriptExecutor.Send"/>
        public void Send(string name, params object[] parameters)
        {
            var len = parameters.Length;
            var fn = _engine.GetFunction(name);
            if (null != fn)
            {
                if (len == 0)
                {
                    fn.Call(_this, _nullArgs);
                }
                else
                {
                    var values = new JsValue[len];
                    for (var i = 0; i < len; i++)
                    {
                        values[i] = JsValue.FromObject(_engine, parameters[i]);
                    }

                    try
                    {
                        fn.Call(_this, values);
                    }
                    catch (JavaScriptException exception)
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
                        _msgMissing.Call(_this, new[] { new JsValue(name) });
                    }
                    catch (JavaScriptException exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                }
                else
                {
                    var values = new[]
                    {
                        new JsValue(name),
                        JsValue.FromObject(_engine, parameters)
                    };

                    try
                    {
                        _msgMissing.Call(_this, values);
                    }
                    catch (JavaScriptException exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                }
            }
        }
    }
}