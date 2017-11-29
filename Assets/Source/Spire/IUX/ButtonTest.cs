﻿using System.Collections.Generic;
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
                                Schema = new ElementSchemaData
                                {
                                    Ints = new Dictionary<string, int>
                                    {
                                        {"ready.frameColor", (int)VirtualColor.Ready },
                                        {"ready.captionColor", (int)VirtualColor.Primary },
                                        {"ready.tween", (int)TweenType.Responsive },

                                        {"activating.frameColor", (int)VirtualColor.Interacting },
                                        {"activating.captionColor", (int)VirtualColor.Interacting },
                                        {"activating.tween", (int)TweenType.Responsive },

                                        {"activated.color", (int)VirtualColor.Interacting },
                                        {"activated.captionColor", (int)VirtualColor.Interacting },
                                        {"activated.tween", (int)TweenType.Instant },
                                    },
                                    Floats = new Dictionary<string, float>
                                    {
                                        { "ready.frameScale", 1.0f },

                                        { "activating.frameScale", 1.1f },

                                        { "activated.frameScale", 1.0f },
                                    }
                                },
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
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}