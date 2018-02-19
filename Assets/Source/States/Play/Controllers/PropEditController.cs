using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    [InjectVine("Prop.Edit")]
    public class PropEditController : InjectableIUXController
    {
        private ElementController _controller;

        [InjectElements("..btn-rename")]
        public ButtonWidget BtnRename { get; private set; }

        [InjectElements("..btn-move")]
        public ButtonWidget BtnMove { get; private set; }

        [InjectElements("..btn-delete")]
        public ButtonWidget BtnDelete { get; private set; }

        [InjectElements("..toggle-fade")]
        public ToggleWidget ToggleFade { get; private set; }

        public event Action<ElementController> OnRename;
        public event Action<ElementController> OnMove;
        public event Action<ElementController> OnDelete;

        public void Initialize(ElementController controller)
        {
            _controller = controller;

            //BtnRename.Activator.OnActivated += Rename_OnActivated;
            BtnMove.Activator.OnActivated += Move_OnActivated;
            BtnDelete.Activator.OnActivated += Delete_OnActivated;
        }

        private void Rename_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnRename)
            {
                OnRename(_controller);
            }
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