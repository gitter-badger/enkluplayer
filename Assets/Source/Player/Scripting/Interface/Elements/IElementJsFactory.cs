using CreateAR.EnkluPlayer.IUX;
using Jint;

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
        /// <param name="engine">The engine to create in.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        ElementJs Instance(Engine engine, Element element);
    }
}