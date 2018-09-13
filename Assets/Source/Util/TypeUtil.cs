using System;
using System.Reflection;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public static class TypeUtil
    {
#if NETFX_CORE
        public static bool IsAssignableFrom(this System.Type @this, System.Type type)
        {
            return @this.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }
#else

        /// <summary>
        /// Allows iteration over all types.
        /// </summary>
        /// <param name="action"></param>
        public static void ForAllTypes(Action<Type> action)
        {
            // catch exceptions
            Assembly[] assemblies;
            try
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            catch (AppDomainUnloadedException appDomainException)
            {
                Debug.LogError("Could not load assemblies : " + appDomainException);
                return;
            }

            for (int i = 0, ilen = assemblies.Length; i < ilen; i++)
            {
                var assembly = assemblies[i];
                Type[] types;

                try
                {
                    // only exported types
                    types = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException typeLoadException)
                {
                    Debug.LogError(typeLoadException);

                    // pull types off of _exception_
                    types = typeLoadException.Types;
                }

                if (null == types)
                {
                    continue;
                }

                // execute action on all types
                for (int j = 0, jlen = types.Length; j < jlen; j++)
                {
                    var type = types[j];

                    action(type);
                }
            }
        }
#endif
    }
}