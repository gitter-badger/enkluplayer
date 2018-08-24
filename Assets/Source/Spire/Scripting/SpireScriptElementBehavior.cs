using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint.Native;
using UnityEngine;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class SpireScriptElementBehavior
    {
        /// <summary>
        /// An engine to run the scripts with.
        /// </summary>
        private UnityScriptingHost _engine;

        /// <summary>
        /// The script to execute.
        /// </summary>
        private SpireScript _script;

        /// <summary>
        /// True iff has been started.
        /// </summary>
        private bool _isStarted;

        /// <summary>
        /// References to JS functions.
        /// </summary>
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
        private readonly JsValue[] _arguments = new JsValue[0];

        /// <summary>
        /// Retrieves the <c>SpireScript</c> instance.
        /// </summary>
        public SpireScript Script { get { return _script; } }

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="element">The element.</param>
        public void Initialize(
            UnityScriptingHost engine,
            SpireScript script,
            Element element)
        {
            if (_isStarted)
            {
                throw new Exception("Script is already running.");
            }

            _engine = engine;
            _script = script;

            var thisBinding = JsValue.FromObject(
                _engine,
                new ElementJs(_engine, null, element));
            _engine.ExecutionContext.ThisBinding = thisBinding;

            try
            {
                _engine.Execute(_script.Program);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not execute script: " + exception);

                return;
            }

            _this = thisBinding;

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

            Log.Info(this, "Entering script {0}.", _script.Data);

            _isStarted = true;

            if (null != _enter)
            {
                _enter.Call(_this, _arguments);
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

            Log.Info(this, "Exiting script {0}.", _script.Data);

            _isStarted = false;

            if (null != _exit)
            {
                _exit.Call(_this, _arguments);
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void Update()
        {
            if (_isStarted && null != _update)
            {
                _update.Call(_this, _arguments);
            }
        }
    }
}