using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Filters elements by type.
    /// </summary>
    public class TypeElementControllerFilter : IElementControllerFilter
    {
        /// <summary>
        /// The type to filter by.
        /// </summary>
        private readonly Type _type;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TypeElementControllerFilter(Type type)
        {
            _type = type;
        }

        /// <inheritdoc />
        public bool Include(Element element)
        {
            return element.GetType() == _type;
        }
    }

    /// <summary>
    /// Filters elements by distance from the camera.
    /// </summary>
    public class DistanceElementControllerFilter : IElementControllerFilter
    {
        /// <inheritdoc />
        public bool Include(Element element)
        {
            return true;
        }
    }
}