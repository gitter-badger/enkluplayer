using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    public class TextBoxWidget : Widget
    {
        public TextBoxWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors)
            : base(gameObject, layers, tweens, colors)
        {

        }
    }
}