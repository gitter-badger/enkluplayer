using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class PropAdjustAxisController : AutoController
    {
        [InjectElements("..(@type==SliderWidget)")]
        public SliderWidget Slider { get; private set; }

        public event Action OnExit;

        protected override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            Slider.OnUnfocused += OnExit;
        }

        protected override void Uninitialize()
        {
            Slider.OnUnfocused -= OnExit;

            base.Uninitialize();
        }
    }
}