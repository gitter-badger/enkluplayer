using CreateAR.EnkluPlayer.Scripting;
using Enklu.Orchid;
using Enklu.Orchid.Jint;
using Jint;

namespace CreateAR.Enkluplayer.Test
{
    public class TestScriptExecutorFactory : IScriptExecutorFactory
    {
        public IJsExecutionContext NewExecutionContext(object context)
        {
            return new JsExecutionContext(new Engine());
        }
    }
}