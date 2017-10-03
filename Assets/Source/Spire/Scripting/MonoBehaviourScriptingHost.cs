using System;
using Jint;
using Jint.Native;
using Jint.Unity;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// This object is able to run a JS script as if it were a MonoBehaviour.
    /// </summary>
    public class MonoBehaviourScriptingHost : MonoBehaviour
    {
        /// <summary>
        /// Properties that need to be serialized for the script.
        /// </summary>
        public ScriptingPropertyBucket Properties;

        /// <summary>
        /// An engine to run the scripts with.
        /// </summary>
        protected Engine _engine;

        /// <summary>
        /// The script to execute.
        /// </summary>
        private SpireScript _script;

        /// <summary>
        /// References to JS functions.
        /// </summary>
        protected ICallable _update;
        protected ICallable _fixedUpdate;
        protected ICallable _lateUpdate;
        protected ICallable _awake;
        protected ICallable _start;
        protected ICallable _onEnable;
        protected ICallable _onDisable;
        protected ICallable _onDestroy;

        /// <summary>
        /// A JS reference to this, passed to every ICallable.
        /// </summary>
        protected JsValue _this;

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        public void Initialize(
            Engine engine,
            SpireScript script)
        {
            _engine = engine;
            _script = script;

            try
            {
                _engine.Execute(_script.Program);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not execute script: " + exception.Message);

                return;
            }

            _this = JsValue.FromObject(_engine, this);

            _update = _engine.GetFunction("Update");
            _fixedUpdate = _engine.GetFunction("FixedUpdate");
            _lateUpdate = _engine.GetFunction("LateUpdate");
            _awake = _engine.GetFunction("Awake");
            _start = _engine.GetFunction("Start");
            _onEnable = _engine.GetFunction("OnEnable");
            _onDisable = _engine.GetFunction("OnDisable");
            _onDestroy = _engine.GetFunction("OnDestroy");

            if (null != _awake)
            {
                _awake.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void Update()
        {
            if (null != _update)
            {
                _update.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void FixedUpdate()
        {
            if (null != _fixedUpdate)
            {
                _fixedUpdate.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void LateUpdate()
        {
            if (null != _lateUpdate)
            {
                _lateUpdate.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void Start()
        {
            if (null != _start)
            {
                _start.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            if (null != _onEnable)
            {
                _onEnable.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void OnDisable()
        {
            if (null != _onDisable)
            {
                _onDisable.Call(_this, null);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void OnDestroy()
        {
            if (null != _onDestroy)
            {
                _onDestroy.Call(_this, null);
            }
        }
    }
}