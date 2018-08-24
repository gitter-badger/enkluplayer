using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A dynamically executable script. 
    /// </summary>
    public class SpireScript
    {
        public enum LoadStatus
        {
            None,
            IsLoading,
            Succeeded,
            Failed
        }

        /// <summary>
        /// Describes an object that executes a script.
        /// </summary>
        public interface IScriptExecutor
        {
            /// <summary>
            /// Data to push into a script.
            /// </summary>
            ElementSchema Data { get; }

            /// <summary>
            /// Sends a message.
            /// </summary>
            /// <param name="name">The name of the message.</param>
            /// <param name="parameters">The parameters.</param>
            void Send(string name, params object[] parameters);
        }

        /// <summary>
        /// For parsing scripts.
        /// </summary>
        private readonly IScriptParser _parser;

        /// <summary>
        /// For loading scripts.
        /// </summary>
        private readonly IScriptLoader _loader;
        
        /// <summary>
        /// Ongoing load.
        /// </summary>
        private IAsyncToken<string> _load;

        /// <summary>
        /// Executes scripts.
        /// </summary>
        private IScriptExecutor _executor;
        
        /// <summary>
        /// Data about the script.
        /// </summary>
        public ScriptData Data { get; private set; }
        
        /// <summary>
        /// Program that can be executed.
        /// </summary>
        public Program Program
        {
            get
            {
                return _parser.Parse(Source, Executor.Data, new ParserOptions());
            }
        }

        /// <summary>
        /// Source code.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Sets an object that is responsible for executing this script.
        /// </summary>
        public IScriptExecutor Executor { get; set; }

        /// <summary>
        /// Load status.
        /// </summary>
        public LoadStatus Status { get; set; }

        public event Action<SpireScript> OnLoadStarted;
        public event Action<SpireScript> OnLoadSuccess;
        public event Action<SpireScript> OnLoadFailure;

        /// <summary>
        /// Creates a new SpireScript.
        /// </summary>
        /// <param name="parser">Parses JS.</param>
        /// <param name="loader">Loads scripts.</param>
        /// <param name="data">Information about the script.</param>
        public SpireScript(
            IScriptParser parser,
            IScriptLoader loader,
            ScriptData data)
        {
            _parser = parser;
            _loader = loader;

            UpdateData(data);
        }
        
        /// <summary>
        /// Should not be called directly. Use <c>IScriptManager</c>::Release().
        /// </summary>
        public void Release()
        {
            if (null != _load)
            {
                _load.Abort();
                _load = null;
            }
        }

        /// <summary>
        /// Updates the script.
        /// </summary>
        /// <param name="data">Data to update.</param>
        public void UpdateData(ScriptData data)
        {
            Log.Info(this, "Spire script data updated.");

            Data = data;
            Status = LoadStatus.IsLoading;

            if (null != _load)
            {
                _load.Abort();
                _load = null;
            }

            if (null != OnLoadStarted)
            {
                OnLoadStarted(this);
            }
            
            _load = _loader
                .Load(Data)
                .OnSuccess(text =>
                {
                    Source = text;
                    Status = LoadStatus.Succeeded;

                    if (null != OnLoadSuccess)
                    {
                        OnLoadSuccess(this);
                    }
                })
                .OnFailure(exception =>
                {
                    Status = LoadStatus.Failed;

                    Log.Error(this, "Could not load script {0} : {1}.",
                        Data,
                        exception);

                    if (null != OnLoadFailure)
                    {
                        OnLoadFailure(this);
                    }
                });
        }

        /// <summary>
        /// Passes a message to script.
        /// </summary>
        /// <param name="name">Name of the message.</param>
        /// <param name="parameters">The parameters.</param>
        public void Send(string name, params object[] parameters)
        {
            if (null != Executor)
            {
                Executor.Send(name, parameters);
            }
            else
            {
                Log.Warning(this, "{0} message sent to {1}, but no Executor could be found.",
                    name,
                    null != Data ? Data.Id : "Unknown");
            }
        }
    }
}