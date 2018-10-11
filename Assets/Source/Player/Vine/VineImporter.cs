using System;
using Antlr4.Runtime.Tree;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Vine;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Creates an <c>ElementDescription</c> from data.
    /// </summary>
    public class VineImporter
    {
        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Preprocess.
        /// </summary>
        private readonly IVinePreProcessor _preProcessor;

        /// <summary>
        /// Loads vines
        /// </summary>
        private readonly VineLoader _loader;

        /// <summary>
        /// Worker.
        /// </summary>
        private readonly ParserWorker _worker;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VineImporter(
            IMetricsService metrics,
            IVinePreProcessor preProcessor,
            VineLoader loader,
            ParserWorker worker)
        {
            _metrics = metrics;
            _preProcessor = preProcessor;
            _loader = loader;
            _worker = worker;
        }

        /// <summary>
        /// Parses a vine definition from source.
        /// </summary>
        /// <param name="data">String data.</param>
        /// <param name="dataStore">Optional data to pass in to parser.</param>
        /// <returns></returns>
        public ElementDescription ParseSync(string data, ElementSchema dataStore = null)
        {
            var document = _loader.Load(data);
            var walker = new ParseTreeWalker();
            var listener = new ElementDescriptionListener();
            walker.Walk(listener, document);

            return listener.Description;
        }
        
        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="data">String data.</param>
        /// <param name="dataStore">Optional data to pass in to preprocessor.</param>
        /// <returns></returns>
        public IAsyncToken<ElementDescription> Parse(string data, ElementSchema dataStore = null)
        {
            // preprocess immediately
            _preProcessor.DataStore = dataStore;
            data = _preProcessor.Execute(data);

            var token = new AsyncToken<ElementDescription>();

            // enqueue some work
            _worker.Enqueue(
                () =>
                {
                    var id = _metrics.Timer(MetricsKeys.SCRIPT_PARSING_VINE).Start();

                    ElementDescription description = null;
                    try
                    {
                        description = ParseSync(data, dataStore);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "Could not read vine : {0}.", exception);
                    }
                    
                    _metrics.Timer(MetricsKeys.SCRIPT_PARSING_VINE).Stop(id);

                    return description;
                },
                token.Succeed);
            

            return token;
        }
    }
}