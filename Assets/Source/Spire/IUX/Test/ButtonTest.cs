using System.Collections.Generic;

namespace CreateAR.SpirePlayer.IUX
{
    public class ButtonTest : InjectableMonoBehaviour
    {
        [Inject]
        public IElementFactory Elements { get; set; }

        private void Start()
        {
            Elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "button",
                    Schema =
                    {
                        Strings = new Dictionary<string, string>
                        {
                            { "label", "Hello World" }
                        },
                        Vectors = new Dictionary<string, Vec3>
                        {
                            { "position", new Vec3(0f, 0.2f, 3f) }
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "button",
                        Type = ElementTypes.BUTTON
                    }
                }
            });
        }
    }
}