using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public class BehaviorScript : Script, EnkluScript.IScriptExecutor
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
        /// Caches JS objects.
        /// </summary>
        private IElementJsCache _jsCache;

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

        /// <inheritdoc />
        public ElementSchema Data
        {
            get { return null == _element ? null : _element.Schema; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BehaviorScript(
            IElementJsCache jsCache,
            IElementJsFactory factory,
            Engine engine,
            EnkluScript script,
            Element element)
        {
            _engine = engine;
            _element = element;
            _jsCache = jsCache;
            _factory = factory;

            EnkluScript = script;
            EnkluScript.Executor = this;
        }

        /// <inheritdoc />
        public override IAsyncToken<Void> Configure()
        {
            var token = new AsyncToken<Void>();
            
            var thisBinding = JsValue.FromObject(
                _engine,
                _factory.Instance(_jsCache, _element));
            _engine.ExecutionContext.ThisBinding = thisBinding;

            try
            {
                Log.Info(this, "Execute : {0}", EnkluScript.Source);
                _engine.Execute(EnkluScript.Program);
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Could not execute script: " + exception);
                token.Fail(exception);
                return token;
            }

            _this = thisBinding;

            _msgMissing = _engine.GetFunction("msgMissing");
            _enter = _engine.GetFunction("enter");
            _update = _engine.GetFunction("update");
            _exit = _engine.GetFunction("exit");

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

            _isStarted = true;

            if (null != _enter)
            {
                try
                {
                    _enter.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
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
                    _update.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
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
                    _exit.Call(_this, _nullArgs);
                }
                catch (JavaScriptException exception)
                {
                    Log.Warning(this, "JavaScript error : {0}.", exception);
                }
            }
        }

        /// <inheritdoc cref="EnkluPlayer.EnkluScript.IScriptExecutor.Send"/>
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