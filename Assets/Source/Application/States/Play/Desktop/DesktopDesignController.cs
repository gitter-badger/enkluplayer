using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design for desktops.
    /// </summary>
    public class DesktopDesignController : IDesignController
    {
        /// <summary>
        /// Transactions.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <inheritdoc />
        public void Setup(PlayModeConfig config, IAppController app)
        {
            //
        }

        /// <inheritdoc />
        public void Teardown()
        {
            //
        }

        public IAsyncToken<string> Create()
        {
            throw new System.NotImplementedException();
        }
    }
}