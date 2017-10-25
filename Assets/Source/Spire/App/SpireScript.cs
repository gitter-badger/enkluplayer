using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint.Parser.Ast;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A dynamically executable script. 
    /// </summary>
    public class SpireScript
    {
        /// <summary>
        /// For parsing scripts.
        /// </summary>
        private readonly IScriptParser _parser;

        /// <summary>
        /// For loading scripts.
        /// </summary>
        private readonly IScriptLoader _loader;
        
        /// <summary>
        /// Backing variable for OnReady.
        /// </summary>
        private readonly MutableAsyncToken<SpireScript> _onReady = new MutableAsyncToken<SpireScript>();

        /// <summary>
        /// Ongoing load.
        /// </summary>
        private IAsyncToken<string> _load;

        /// <summary>
        /// Data about the script.
        /// </summary>
        public ScriptData Data { get; private set; }

        /// <summary>
        /// Token when script is available to execute.
        /// </summary>
        public IMutableAsyncToken<SpireScript> OnReady { get { return _onReady; } }

        /// <summary>
        /// Program that can be executed.
        /// </summary>
        public Program Program { get; private set; }

        /// <summary>
        /// Source code.
        /// </summary>
        public string Source { get; private set; }

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
            Data = data;

            if (null != _load)
            {
                _load.Abort();
                _load = null;
            }
            
            _load = _loader
                .Load(Data)
                .OnSuccess(OnLoaded)
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load script {0} : {1}.",
                        Data,
                        exception);

                    _onReady.Fail(exception);
                });
        }

        /// <summary>
        /// Called when the script has been downloaded.
        /// </summary>
        /// <param name="text">Text of the script.</param>
        private void OnLoaded(string text)
        {
            Log.Info(this, "Script loaded, parsing Program : {0}.",
                Data);

            Source = text;

            // parse!
            _parser
                .Parse(Source)
                .OnSuccess(program =>
                {
                    Log.Info(this, "Script parsed and ready.");

                    Program = program;

                    _onReady.Succeed(this);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not parse {0} : {1}.",
                        Data,
                        exception);

                    _onReady.Fail(exception);
                });
        }
    }
}