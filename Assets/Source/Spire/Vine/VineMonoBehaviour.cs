using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public class VineMonoBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Imports from vine.
        /// </summary>
        private readonly VineImporter _importer = new VineImporter(new JsVinePreProcessor());

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; private set; }

        /// <summary>
        /// The vine.
        /// </summary>
        public TextAsset Vine;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Start()
        {
            if (null != Vine)
            {
                Elements.Element(_importer.Parse(Vine.text));
            }
        }
    }
}