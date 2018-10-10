using Antlr4.Runtime.Tree;
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
        /// Constructor.
        /// </summary>
        public VineImporter(
            IMetricsService metrics,
            IVinePreProcessor preProcessor,
            VineLoader loader)
        {
            _metrics = metrics;
            _preProcessor = preProcessor;
            _loader = loader;
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="data">String data.</param>
        /// <param name="dataStore">Optional data to pass in to preprocessor.</param>
        /// <returns></returns>
        public ElementDescription Parse(string data, ElementSchema dataStore = null)
        {
            var id = _metrics.Timer(MetricsKeys.SCRIPT_PARSING_VINE).Start();

            _preProcessor.DataStore = dataStore;
            data = _preProcessor.Execute(data);

            var document = _loader.Load(data);

            var walker = new ParseTreeWalker();
            var listener = new ElementDescriptionListener();
            walker.Walk(listener, document);

            _metrics.Timer(MetricsKeys.SCRIPT_PARSING_VINE).Stop(id);

            return listener.Description;
        }
    }
}