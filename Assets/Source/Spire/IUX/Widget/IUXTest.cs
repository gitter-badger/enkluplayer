

using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class IUXTest : InjectableMonoBehaviour
    {
        /// <summary>
        /// Test button
        /// </summary>
        public Button Button;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var buttonSchema = new ButtonSchema()
            {
                Caption = new TextSchema()
                {
                    Text = "Hello World",
                    FontSize = 42,
                    AnchorPosition = WidgetAnchorPosition.Right
                },

                Highlight = true,
                VoiceActivator = "Banana"
            };

            Button.SetSchema(buttonSchema);
        }
    }
}
