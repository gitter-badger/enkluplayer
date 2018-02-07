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

        [InjectElements("..slider-x")]
        public SliderWidget SliderX { get; private set; }

        [InjectElements("..slider-y")]
        public SliderWidget SliderY { get; private set; }

        [InjectElements("..slider-z")]
        public SliderWidget SliderZ { get; private set; }

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

            SliderX.OnUnfocused += SliderX_OnUnfocused;
            SliderY.OnUnfocused += SliderY_OnUnfocused;
            SliderZ.OnUnfocused += SliderZ_OnUnfocused;
        }

        private void SliderX_OnUnfocused()
        {
            SliderX.LocalVisible = false;
            Container.LocalVisible = true;
        }

        private void SliderY_OnUnfocused()
        {
            SliderY.LocalVisible = false;
            Container.LocalVisible = true;
        }

        private void SliderZ_OnUnfocused()
        {
            SliderZ.LocalVisible = false;
            Container.LocalVisible = true;
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
            SliderX.LocalVisible = true;
        }

        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
            SliderY.LocalVisible = true;
        }

        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            Container.LocalVisible = false;
            SliderZ.LocalVisible = true;
        }
    }
}