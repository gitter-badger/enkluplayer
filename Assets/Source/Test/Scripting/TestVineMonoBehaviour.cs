using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Mimics a VineMonoBehaviour and counts invokes on general script flow calls.
    /// </summary>
    public class TestVineMonoBehaviour : VineScript
    {
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }

        private readonly AsyncToken<Void> _configureToken = new AsyncToken<Void>();

        public override void Initialize(
            Element parent,
            EnkluScript script,
            VineImporter importer,
            IElementFactory elements)
        {
            
        }

        public override IAsyncToken<Void> Configure()
        {
            return _configureToken;
        }

        /// <inheritdoc />
        public override void Enter()
        {
            base.Enter();
            EnterInvoked++;
        }

        public override void FrameUpdate()
        {
            
        }

        public override void Exit()
        {
            base.Exit();
            ExitInvoked++;
        }

        /// <summary>
        /// Marks configuring complete, and resolves the pending AsyncToken.
        /// </summary>
        public void FinishConfigure()
        {
            _configureToken.Succeed(Void.Instance);
        }
    }
}
