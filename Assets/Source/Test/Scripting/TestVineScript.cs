using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Mimics a VineMonoBehaviour and counts invokes on general script flow calls.
    /// </summary>
    public class TestVineScript : VineScript
    {
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }

        private readonly AsyncToken<Void> _configureToken = new AsyncToken<Void>();

        public TestVineScript(EnkluScript script) : base(null, script, null, null)
        {
            EnkluScript = script;
        }

        public override IAsyncToken<Void> Configure()
        {
            return _configureToken;
        }

        /// <inheritdoc />
        public override void Enter()
        {
            EnterInvoked++;
        }

        public override void FrameUpdate()
        {
            
        }

        public override void Exit()
        {
            ExitInvoked++;
        }

        /// <summary>
        /// Marks configuring complete, and resolves the pending AsyncToken.
        /// </summary>
        public void FinishConfigure()
        {
            IsConfigured = true;
            _configureToken.Succeed(Void.Instance);
        }
    }
}
