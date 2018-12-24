using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Jint;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Mimics an EnkluScriptElementBehavior and counts invokes on general script flow calls.
    /// </summary>
    public class TestBehaviorMonoBehaviour : BehaviorScript
    {
        private static int _enterInvokeId = 0;
        private static int _exitInvokeId = 0;
        
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }
        public int LastEnterInvokeId { get; private set; }
        public int LastExitInvokeUd { get; private set; } 
        
        public override void Initialize(IElementJsCache jsCache, IElementJsFactory factory, Engine engine, EnkluScript script, Element element)
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
            base.Enter();
            EnterInvoked++;
            LastEnterInvokeId = _enterInvokeId++;
        }

        public override void FrameUpdate()
        {
            
        }

        public override void Exit()
        {
            base.Exit();
            ExitInvoked++;
            LastExitInvokeUd = _exitInvokeId++;
        }
        
        /// <summary>
        /// Resets the internal shared invoke counter to 0.
        /// </summary>
        public static void ResetInvokeIds()
        {
            _enterInvokeId = 0;
        }
    }
}