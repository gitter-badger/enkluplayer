using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class Menu : Widget
    {
        private readonly IPrimitiveFactory _primitives;

        public Menu(
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                  new GameObject("Menu"),
                  config,
                  layers,
                  tweens,
                  colors,
                  messages)
        {
            _primitives = primitives;
        }

        protected override void LoadInternal()
        {
            base.LoadInternal();

            
        }
    }
}