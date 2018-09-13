using System;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Element utilities.
    /// </summary>
    public static class ElementUtil
    {
        /// <summary>
        /// Depth first walk of element tree.
        /// </summary>
        /// <param name="element">Starting element.</param>
        /// <param name="func">The func.</param>
        public static void Walk(
            Element element,
            Action<Element> func)
        {
            func(element);

            var children = element.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Walk(children[i], func);
            }
        }
    }
}