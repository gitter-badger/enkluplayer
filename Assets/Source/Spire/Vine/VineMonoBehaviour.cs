using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class VineMonoBehaviour : InjectableMonoBehaviour
    {
        private readonly VineImporter _importer = new VineImporter();

        [Inject]
        public IElementFactory Elements { get; private set; }

        public TextAsset Vine;

        private void Start()
        {
            if (null != Vine)
            {
                Elements.Element(_importer.Parse(Vine.text));
            }
        }
    }
}