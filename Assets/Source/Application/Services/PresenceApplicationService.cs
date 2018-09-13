using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    public class PresenceApplicationService : ApplicationService
    {
        public PresenceApplicationService(MessageTypeBinder binder, IMessageRouter messages) : base(binder, messages)
        {
        }
    }
}