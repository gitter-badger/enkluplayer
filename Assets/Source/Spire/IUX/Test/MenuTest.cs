using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class MenuTest : MonoBehaviour
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
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "menu",
                        Type = ElementTypes.MENU
                    },
                    new ElementData
                    {
                        Id = "button",
                        Type = ElementTypes.BUTTON,
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
                                {"ready.frameScale", 1.0f},

                                {"activating.frameScale", 1.1f},

                                {"activated.frameScale", 1.0f},
                            }
                        }
                    }
                }
            });
        }
    }
}