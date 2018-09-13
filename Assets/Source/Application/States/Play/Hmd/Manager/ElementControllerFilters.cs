using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
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
            var elementType = element.GetType();

            return _type == elementType
                || _type.IsAssignableFrom(elementType);
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