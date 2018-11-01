using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptService : ApplicationService
    {
        public ScriptService(
            MessageTypeBinder binder, 
            IMessageRouter messages,
            IElementManager elementManager) : base(binder, messages)
        {
            
        }
    }
}