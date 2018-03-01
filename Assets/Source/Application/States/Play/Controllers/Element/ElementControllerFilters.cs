using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class TypeElementControllerFilter : IElementControllerFilter
    {
        private readonly Type _type;

        public TypeElementControllerFilter(Type type)
        {
            _type = type;
        }

        public bool Include(Element element)
        {
            return element.GetType() == _type;
        }
    }

    public class DistanceElementControllerFilter : IElementControllerFilter
    {
        public bool Include(Element element)
        {
            return true;
        }
    }
}