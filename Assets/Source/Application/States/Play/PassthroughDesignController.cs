using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// IDesignController that does nothing.
    /// </summary>
    public class PassthroughDesignController : IDesignController
    {
        private DesignControllerMode _mode = DesignControllerMode.Normal;
        
        /// <inheritdoc />
        public DesignControllerMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        
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