using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Jint.Unity;
using strange.extensions.injector.api;

namespace CreateAR.SpirePlayer
{
    public class SpireScriptInjector : IScriptDependencyResolver
    {
        private readonly IInjectionBinder _binder;

        private readonly Dictionary<string, object> _registrations = new Dictionary<string, object>();

        public SpireScriptInjector(IInjectionBinder binder)
        {
            _binder = binder;
        }

        public SpireScriptInjector Bind(string name, object @object)
        {
            if (_registrations.ContainsKey(name))
            {
                throw new Exception(string.Format("Binding already exists for {0}.", name));
            }

            _registrations[name] = @object;

            return this;
        }

        public object Resolve(string name)
        {
            object @object;
            if (_registrations.TryGetValue(name, out @object))
            {
                return @object;
            }

            Log.Warning(this, "Could not find script dependency : {0}.", name);
            return null;
        }
    }
}