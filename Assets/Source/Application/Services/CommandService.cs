using System;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Base implementation of service that receives commands.
    /// </summary>
    public class CommandService : ApplicationService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommandService(
            MessageTypeBinder binder,
            IMessageRouter messages)
            : base(binder, messages)
        {
            //
        }

        /// <summary>
        /// Sets a handler by type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="handler">The associated handler.</param>
        public virtual void SetHandler(string type, Action<string, ICommandClient> handler)
        {
            //
        }

        /// <summary>
        /// Removes a handler.
        /// </summary>
        /// <param name="type">The type the handler was associated with.</param>
        public virtual void RemoveHandler(string type)
        {
            //
        }
    }
}