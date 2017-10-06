using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Decorates a class that can be resolved in a require().
    /// 
    /// These objects should be singletons!
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true,
        Inherited = false)]
    public class JsInterfaceAttribute : Attribute
    {
        /// <summary>
        /// Name to use (i.e. value passed to require()).
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Value passed to require.</param>
        public JsInterfaceAttribute(string name)
        {
            Name = name;
        }
    }
}