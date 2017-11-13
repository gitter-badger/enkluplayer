

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
                    Id = "caption",
                    Schema = new ElementSchemaData()
                    {
                        Ints = new Dictionary<string, int>()
                        {
                            {"type", ElementTypes.CAPTION}
                        }
                    }
                }
            };

            var elementDescription
                = new ElementDescription()
                {
                    Elements = elementPrefabs,

                    Root = new ElementRef()
                    {
                        Id = "caption",
                        Schema = new ElementSchemaData()
                        {
                            Strings = new Dictionary<string, string>()
                            {
                                { "name", "New Caption" },
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
