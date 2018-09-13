using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Base class for rendering props.
    /// </summary>
    public abstract class PropRenderer
    {
        /// <summary>
        /// Draws controls for a prop, returns true if repaint is needed.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <returns></returns>
        public abstract bool Draw(ElementSchemaProp prop);
    }
}