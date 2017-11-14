
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
                                { "text", "Hello World!" },
                            },
                            Ints = new Dictionary<string, int>()
                            { 
                                { "fontSize", 12 },
                            },
                            Vectors = new Dictionary<string, Vec3>()
                            { 
                                { "position", new Vec3(0,0,3) }
                            },
                            Colors = new Dictionary<string, Col4>()
                            {
                                { "color" , new Col4(1, 1, 1, 1) }
                            }
                        }
                    }
                };

            var button = ElementFactory.Element(buttonDescription);
        }
    }
}
