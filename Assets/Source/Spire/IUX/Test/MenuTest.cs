using System.Collections.Generic;

namespace CreateAR.SpirePlayer.IUX
{
    public class MenuTest : InjectableMonoBehaviour
    {
        [Inject]
        public IElementFactory Elements { get; set; }

        private void Start()
        {
            Elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "menu",
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "menu",
                        Type = ElementTypes.MENU,
                        Schema =
                        {
                            Strings = new Dictionary<string, string>
                            {
                                { "label", "Choose an option!" }
                            },
                            Ints = new Dictionary<string, int>
                            {
                                { "fontSize", 80 }
                            },
                            Vectors = new Dictionary<string, Vec3>
                            {
                                { "position", new Vec3(0f, 0.2f, 3f) }
                            }
                        },
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = "button",
                                Type = ElementTypes.BUTTON,
                                Schema =
                                {
                                    Strings = new Dictionary<string, string>
                                    {
                                        { "label", "A" }
                                    }
                                }
                            },
                            new ElementData
                            {
                                Id = "button",
                                Type = ElementTypes.BUTTON,
                                Schema =
                                {
                                    Strings = new Dictionary<string, string>
                                    {
                                        { "label", "B" }
                                    }
                                }
                            },
                            new ElementData
                            {
                                Id = "button",
                                Type = ElementTypes.BUTTON,
                                Schema =
                                {
                                    Strings = new Dictionary<string, string>
                                    {
                                        { "label", "C" }
                                    }
                                }
                            }
                        }
                    },
                }
            });
        }
    }
}