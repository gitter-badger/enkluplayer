using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Describes an object that makes <c>ElementJs</c> instances.
    /// </summary>
    public interface IElementJsFactory
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="cache">Js cache.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        ElementJs Instance(IElementJsCache cache, Element element);
    }
}