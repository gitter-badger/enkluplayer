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
                                { "title", "Welcome" },
                                { "description", "Move the cursor with your head to activate the Ok button to the right." },
                            },
                            Floats = new Dictionary<string, float>
                            {
                                
                            },
                            Ints = new Dictionary<string, int>
                            {
                                
                            },
                            Vectors = new Dictionary<string, Vec3>
                            {
                                { "position", new Vec3(0f, 0f, 3f) }
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
                                        { "label", "Ok" }
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
                                        { "label", "Login" }
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
                                        { "label", "Explore" }
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
                                        { "label", "Options" }
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