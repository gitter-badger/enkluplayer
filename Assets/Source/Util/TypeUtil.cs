using System.Reflection;

namespace CreateAR.SpirePlayer
{
    public static class TypeUtil
    {
#if NETFX_CORE
        public static bool IsAssignableFrom(this System.Type @this, System.Type type)
        {
            return @this.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }
#endif
    }
}