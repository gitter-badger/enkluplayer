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
                        Schema =
                        {
                            Ints = new Dictionary<string, int>
                            {
                                {"type", ElementTypes.BUTTON}
                            }
                        },
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = "activator",
                                Schema = new ElementSchemaData
                                {
                                    Ints = new Dictionary<string, int>
                                    {
                                        {"type", ElementTypes.ACTIVATOR}
                                    }
                                },
                                Children = new []
                                {
                                    new ElementData
                                    {
                                        Id = "caption",
                                        Schema = new ElementSchemaData
                                        {
                                            Ints = new Dictionary<string, int>
                                            {
                                                {"type", ElementTypes.CAPTION}
                                            },
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
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"type", ElementTypes.BUTTON_READY_STATE },
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
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"type", ElementTypes.BUTTON_ACTIVATING_STATE },
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
                                                Schema = new ElementSchemaData
                                                {
                                                    Ints = new Dictionary<string, int>
                                                    {
                                                        {"type", ElementTypes.BUTTON_ACTIVATED_STATE },
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