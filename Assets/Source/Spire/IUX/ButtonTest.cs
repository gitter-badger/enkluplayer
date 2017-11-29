using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
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
                            { "text", "Hello World" }
                        },
                        Ints = new Dictionary<string, int>
                        {
                            { "fontSize", 80 }
                        },
                        Vectors = new Dictionary<string, Vec3>
                        {
                            { "position", new Vec3(0,0,3) }
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "button",
                        Type = ElementTypes.BUTTON,
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = "activator",
                                Type = ElementTypes.ACTIVATOR,
                                Children = new []
                                {
                                    new ElementData
                                    {
                                        Id = "caption",
                                        Type = ElementTypes.CAPTION,
                                        Schema = new ElementSchemaData
                                        {
                                            Strings = new Dictionary<string, string>
                                            {
                                                { "name", "Button Caption" },
                                            },
                                            Vectors = new Dictionary<string, Vec3>
                                            {
                                                { "position", new Vec3(0.2f,0,0) }
                                            }
                                        }
                                    },
                                    new ElementData
                                    {
                                        Id = "states",
                                        Children = new[]
                                        {
                                            new ElementData
                                            {
                                                Id = "ready",
                                                Type = ElementTypes.BUTTON_READY_STATE,
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"color", (int)VirtualColor.Ready },
                                                        {"captionColor", (int)VirtualColor.Primary },
                                                        {"tween", (int)TweenType.Responsive }
                                                    },
                                                    Floats = new Dictionary<string, float>
                                                    {
                                                        { "frameScale", 1.0f }
                                                    }
                                                }
                                            },

                                            new ElementData
                                            {
                                                Id = "activating",
                                                Type = ElementTypes.BUTTON_ACTIVATING_STATE,
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"color", (int)VirtualColor.Interacting },
                                                        {"captionColor", (int)VirtualColor.Interacting },
                                                        {"tween", (int)TweenType.Responsive },
                                                    },
                                                    Floats = new Dictionary<string, float>
                                                    {
                                                        { "frameScale", 1.1f }
                                                    }
                                                }
                                            },

                                            new ElementData
                                            {
                                                Id = "activated",
                                                Type = ElementTypes.BUTTON_ACTIVATED_STATE,
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"color", (int)VirtualColor.Interacting },
                                                        {"captionColor", (int)VirtualColor.Interacting },
                                                        {"tween", (int)TweenType.Instant },
                                                    },
                                                    Floats = new Dictionary<string, float>
                                                    {
                                                        { "frameScale", 1.0f }
                                                    }
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}