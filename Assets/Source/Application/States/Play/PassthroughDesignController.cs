using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// IDesignController that does nothing.
    /// </summary>
    public class PassthroughDesignController : IDesignController
    {
        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            
        }

        /// <inheritdoc />
        public void Teardown()
        {
            
        }

        /// <inheritdoc />
        public IAsyncToken<string> Create()
        {
            return new AsyncToken<string>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Select(string sceneId, string elementId)
        {
            
        }

        /// <inheritdoc />
        public void Focus(string sceneId, string elementId)
        {
            
        }
    }
}