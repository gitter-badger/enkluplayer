using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
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