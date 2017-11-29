using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public class CursorTest : InjectableMonoBehaviour
    {
        [Inject]
        public IElementFactory Elements { get; set; }

        private void Start()
        {
            Elements.Element(new ElementDescription
            {
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "cursor",
                        Schema = new ElementSchemaData
                        {
                            Ints = new Dictionary<string, int>
                            {
                                {"type", ElementTypes.CURSOR}
                            }
                        },
                        Children = new[]
                        {
                            new ElementData
                            {
                                Id = "reticle",
                                Schema = new ElementSchemaData
                                {
                                    Ints = new Dictionary<string, int>
                                    {
                                        {"type", ElementTypes.RETICLE}
                                    }
                                },
                            }
                        }
                    },
                },

                Root = new ElementRef
                {
                    Id = "cursor",
                    Schema = new ElementSchemaData
                    {
                        Strings = new Dictionary<string, string>
                        {
                            {"name", "Cursor"},
                        }
                    }
                }
            });
        }
    }
}