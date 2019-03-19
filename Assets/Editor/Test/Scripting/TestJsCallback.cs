using System;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestJsCallback : IJsCallback
    {
        private readonly Func<object, object[], object> _handler;
        public TestJsCallback(Func<object, object[], object> handler)
        {
            _handler = handler;
        }

        public object Apply(object @this, params object[] args)
        {
            return _handler(@this, args);
        }

        public object Invoke(params object[] args)
        {
            return _handler(null, args);
        }

        public IJsExecutionContext ExecutionContext { get; private set; }
    }
}