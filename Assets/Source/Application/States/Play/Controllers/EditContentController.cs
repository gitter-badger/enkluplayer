using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controller for editing a prop.
    /// </summary>
    [InjectVine("Content.Edit")]
    public class EditContentController : InjectableIUXController
    {
        private ContentDesignController _controller;
        
        [InjectElements("..btn-move")]
        public ButtonWidget BtnMove { get; private set; }

        [InjectElements("..btn-delete")]
        public ButtonWidget BtnDelete { get; private set; }

        [InjectElements("..toggle-fade")]
        public ToggleWidget ToggleFade { get; private set; }
        
        public event Action<ContentDesignController> OnMove;
        public event Action<ContentDesignController> OnDelete;

        public void Initialize(ContentDesignController controller)
        {
            _controller = controller;
            
            BtnMove.Activator.OnActivated += Move_OnActivated;
            BtnDelete.Activator.OnActivated += Delete_OnActivated;
        }
        
        private void Move_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnMove)
            {
                OnMove(_controller);
            }
        }

        private void Delete_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnDelete)
            {
                OnDelete(_controller);
            }
        }
    }
}