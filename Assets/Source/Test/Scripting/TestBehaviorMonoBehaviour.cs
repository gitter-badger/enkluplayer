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
        private static int _invokeId = 0;
        
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }
        public int LastInvokeId { get; private set; }
        
        public override void Initialize(IElementJsCache jsCache, IElementJsFactory factory, Engine engine, EnkluScript script, Element element)
        {
            
        }

        public override IAsyncToken<Void> Configure()
        {
            var token = new AsyncToken<Void>();
            token.Succeed(Void.Instance);
            return token;
        }

        public override void Enter()
        {
            EnterInvoked++;
            LastInvokeId = _invokeId++;
        }

        public override void FrameUpdate()
        {
            
        }

        public override void Exit()
        {
            ExitInvoked++;
            LastInvokeId = _invokeId++;
        }
        
        /// <summary>
        /// Resets the internal shared invoke counter to 0.
        /// </summary>
        public static void ResetInvokeIds()
        {
            _invokeId = 0;
        }
    }
}