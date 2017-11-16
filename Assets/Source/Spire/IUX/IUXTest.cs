
using CreateAR.SpirePlayer.UI;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class IUXTest : InjectableMonoBehaviour
    {
        /// <summary>
        /// Element Creation
        /// </summary>
        [Inject]
        public IElementFactory ElementFactory { get; set; } 
        
        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var elementPrefabs = new[]
            {
                new ElementData()
                {
                    Id = "cursor",
                    Schema = new ElementSchemaData()
                    {
                        Ints = new Dictionary<string, int>()
                        {
                            {"type", ElementTypes.CURSOR}
                        }
                    }
                },
                new ElementData()
                {
                    Id = "caption",
                    Schema = new ElementSchemaData()
                    {
                        Ints = new Dictionary<string, int>()
                        {
                            {"type", ElementTypes.CAPTION}
                        }
                    }
                },
                new ElementData()
                {
                    Id = "button",
                    Schema = new ElementSchemaData()
                    {
                        Ints = new Dictionary<string, int>()
                        {
                            {"type", ElementTypes.BUTTON}
                        }
                    },
                    Children = new ElementData[]
                    {
                        new ElementData()
                        {
                            Id = "caption",
                            Schema = new ElementSchemaData()
                            {
                                Ints = new Dictionary<string, int>()
                                {
                                    {"type", ElementTypes.CAPTION}
                                },
                                Strings = new Dictionary<string, string>()
                                {
                                    { "name", "Button Caption" },
                                },
                                Vectors = new Dictionary<string, Vec3>()
                                {
                                    { "position", new Vec3(0.2f,0,0) }
                                }
                            }
                        }
                    }
                }
            };

            var cursorDescription
                = new ElementDescription()
                {
                    Elements = elementPrefabs,

                    Root = new ElementRef()
                    {
                        Id = "cursor",
                        Schema = new ElementSchemaData()
                        {
                            Strings = new Dictionary<string, string>()
                            {
                                {"name", "Cursor"},
                            }
                        }
                    }
                };

            var cursor = ElementFactory.Element(cursorDescription);

            var buttonDescription
                = new ElementDescription()
                {
                    Elements = elementPrefabs,

                    Root = new ElementRef()
                    {
                        Id = "button",
                        Schema = new ElementSchemaData()
                        {
                            Strings = new Dictionary<string, string>()
                            {
                                { "name", "New Button" },
                            },
                            Ints = new Dictionary<string, int>()
                            { 
                                { "fontSize", 12 },
                            },
                            Vectors = new Dictionary<string, Vec3>()
                            { 
                                { "position", new Vec3(0,0,3) }
                            }
                        },
                        Children = new ElementRef[]
                        {
                            new ElementRef()
                            {
                                Id = "caption",
                                Schema = new ElementSchemaData()
                                {
                                    Strings = new Dictionary<string, string>()
                                    {
                                        { "text", "Hello World!" },
                                    }
                                },
                            }
                        }
                    }
                };

            var button = ElementFactory.Element(buttonDescription);
        }
    }
}
