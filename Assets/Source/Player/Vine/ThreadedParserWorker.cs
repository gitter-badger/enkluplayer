using CreateAR.Commons.Unity.Http;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Named worker for parsing.
    /// </summary>
    public class ThreadedParserWorker : ActionWorker, IParserWorker
    {
        /// <summary>
        /// Constructor.
        /// </summary>?
        public ThreadedParserWorker(IBootstrapper bootstrapper)
            : base(bootstrapper)
        {
            //
        }
    }
}