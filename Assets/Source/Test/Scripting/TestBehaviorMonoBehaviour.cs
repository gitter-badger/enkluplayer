using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Jint;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestBehaviorMonoBehaviour : BehaviorScript
    {
        public int EnterInvoked { get; private set; }
        public int ExitInvoked { get; private set; }
        
        public override void Initialize(IElementJsCache jsCache, IElementJsFactory factory, Engine engine, EnkluScript script, Element element)
        {
            
        }

        public override void Configure()
        {
            
        }

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
    }
}