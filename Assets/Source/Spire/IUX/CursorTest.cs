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
                        Type = ElementTypes.CURSOR
                    },
                },

                Root = new ElementRef
                {
                    Id = "cursor"
                }
            });
        }
    }
}