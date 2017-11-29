using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public class TextTest : InjectableMonoBehaviour
    {
        [Inject]
        public IElementFactory Elements { get; set; }
        
        private void Start()
        {
            Elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "caption",
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
                        Vectors = new Dictionary<string, Vec3>()
                        {
                            { "position", new Vec3(0,0,3) }
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "caption",
                        Schema =
                        {
                            Ints = new Dictionary<string, int>()
                            {
                                {"type", ElementTypes.CAPTION}
                            }
                        }
                    } 
                }
            });
        }
    }
}