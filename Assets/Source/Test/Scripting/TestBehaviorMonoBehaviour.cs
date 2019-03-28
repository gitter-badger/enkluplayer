using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Jint;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Mimics an EnkluScriptElementBehavior and counts invokes on general script flow calls.
    /// </summary>
    public class TestBehaviorScript : BehaviorScript
    {
        private static int _enterInvokeId = 0;
        private static int _exitInvokeId = 0;
        private static int _updateInvokeId = 0;
        
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }
        public int UpdateInvoked { get; private set; }
        public int LastEnterInvokeId { get; private set; }
        public int LastExitInvokeId { get; private set; }
        public int LastUpdateInvokeId { get; private set; }
        
        public TestBehaviorScript(EnkluScript script) : base(null, null, script, null)
        {
            EnkluScript = script;
        }

        public override IAsyncToken<Void> Configure()
        {
            var token = new AsyncToken<Void>();
            token.Succeed(Void.Instance);
            IsConfigured = true;
            return token;
        }

        public override void Enter()
        {
            EnterInvoked++;
            LastEnterInvokeId = _enterInvokeId++;
        }

        public override void FrameUpdate()
        {
            UpdateInvoked++;
            LastUpdateInvokeId = _updateInvokeId++;
        }

        public override void Exit()
        {
            ExitInvoked++;
            LastExitInvokeId = _exitInvokeId++;
        }
        
        /// <summary>
        /// Resets the internal shared invoke counter to 0.
        /// </summary>
        public static void ResetInvokeIds()
        {
            _enterInvokeId = 0;
            _exitInvokeId = 0;
            _updateInvokeId = 0;
        }
    }
}