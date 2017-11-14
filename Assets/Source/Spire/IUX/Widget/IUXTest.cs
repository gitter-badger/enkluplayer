
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

            var elementDescription
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
                                { "fontSize", "12" }
                            }
                        }
                    }
                };

            var element = ElementFactory.Element(elementDescription);
        }
    }
}
