using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    [InjectVine("Prop.Adjust")]
    public class PropAdjustController : InjectableIUXController
    {
        private PropController _controller;

        [InjectElements("..controls")]
        public Widget Container { get; private set; }

        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; private set; }

        [InjectElements("..btn-rotate")]
        public ButtonWidget BtnRotate{ get; private set; }

        [InjectElements("..btn-scale")]
        public ButtonWidget BtnScale { get; private set; }

        [InjectElements("..btn-x")]
        public ButtonWidget BtnX { get; private set; }

        [InjectElements("..btn-y")]
        public ButtonWidget BtnY { get; private set; }

        [InjectElements("..btn-z")]
        public ButtonWidget BtnZ { get; private set; }
        
        public event Action OnExit;

        public void Initialize(PropController controller)
        {
            _controller = controller;

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnRotate.Activator.OnActivated += BtnRotate_OnActivated;
            BtnScale.Activator.OnActivated += BtnScale_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;
        }
        
        private void BtnBack_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnExit)
            {
                OnExit();
            }
        }

        private void BtnRotate_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            
        }

        private void BtnScale_OnActivated(ActivatorPrimitive activatorPrimitive)
        {

        }

        private void BtnX_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
        }

        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
        }

        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
        }
    }
}