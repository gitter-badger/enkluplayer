using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint.Native;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class SpireScriptElementBehavior : SpireScript.IScriptExecutor
    {
        /// <summary>
        /// An engine to run the scripts with.
        /// </summary>
        private UnityScriptingHost _engine;

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

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="factory">Creates elements.</param>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="element">The element.</param>
        public void Initialize(
            IElementJsFactory factory,
            UnityScriptingHost engine,
            SpireScript script,
            Element element)
        {
            if (_isStarted)
            {
                throw new Exception("Script is already running.");
            }

            _engine = engine;

            Script = script;
            Script.Executor = this;
            
            Run(factory, element);
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

            Log.Info(this, "Entering script {0}.", Script.Data);

            _isStarted = true;

            if (null != _enter)
            {
                _enter.Call(_this, _nullArgs);
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

            Log.Info(this, "Exiting script {0}.", Script.Data);

            _isStarted = false;

            if (null != _exit)
            {
                _exit.Call(_this, _nullArgs);
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void Update()
        {
            if (_isStarted && null != _update)
            {
                _update.Call(_this, _nullArgs);
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

                    fn.Call(_this, values);
                }
            }
            else if (null != _msgMissing)
            {
                if (parameters.Length == 0)
                {
                    _msgMissing.Call(_this, new []{ new JsValue(name) });
                }
                else
                {
                    var values = new[]
                    {
                        new JsValue(name),
                        JsValue.FromObject(_engine, parameters)
                    };

                    _msgMissing.Call(_this, values);
                }
            }
        }

        /// <summary>
        /// Inits the script and runs it.
        /// </summary>
        private void Run(IElementJsFactory factory, Element element)
        {
            var thisBinding = JsValue.FromObject(
                _engine,
                factory.Instance(_engine, element));
            _engine.ExecutionContext.ThisBinding = thisBinding;

            // common apis
            _engine.SetValue("v", Vec3Methods.Instance);
            _engine.SetValue("vec3", new Func<float, float, float, Vec3>(Vec3Methods.create));
            _engine.SetValue("q", QuatMethods.Instance);
            _engine.SetValue("quat", new Func<float, float, float, float, Quat>(QuatMethods.create));
            _engine.SetValue("time", TimeJsApi.Instance);

            try
            {
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
    }
}