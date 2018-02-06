using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class PropAdjustController : AutoController
    {
        private ContentWidget _content;

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

        protected override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _content = (ContentWidget) context;

            BtnBack.Activator.OnActivated += BtnBack_OnActivated;

            BtnRotate.Activator.OnActivated += BtnRotate_OnActivated;
            BtnScale.Activator.OnActivated += BtnScale_OnActivated;

            BtnX.Activator.OnActivated += BtnX_OnActivated;
            BtnY.Activator.OnActivated += BtnY_OnActivated;
            BtnZ.Activator.OnActivated += BtnZ_OnActivated;
        }
        
        protected override void Uninitialize()
        {


            base.Uninitialize();
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

        }

        private void BtnY_OnActivated(ActivatorPrimitive activatorPrimitive)
        {

        }

        private void BtnZ_OnActivated(ActivatorPrimitive activatorPrimitive)
        {

        }
    }
}