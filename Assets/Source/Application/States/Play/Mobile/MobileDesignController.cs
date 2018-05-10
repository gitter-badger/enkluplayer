using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Designer for mobile.
    /// </summary>
    public class MobileDesignController : IDesignController
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
            //
        }
    }
}