using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public abstract class VineScript : Script
    {
        /// <summary>
        /// Initializes script.
        /// </summary>
        public abstract void Initialize(
            Element parent,
            EnkluScript script,
            VineImporter importer,
            IElementFactory elements);
    }
}
