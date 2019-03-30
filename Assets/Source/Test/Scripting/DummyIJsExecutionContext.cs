using System;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class DummyIJsExecutionContext : IJsExecutionContext
    {
        public IJsModule NewModule(string moduleId)
        {
            return new DummyIJsModule();
        }

        public T GetValue<T>(string name)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(string name, T value)
        {
            throw new NotImplementedException();
        }

        public void RunScript(string script)
        {
            throw new NotImplementedException();
        }

        public void RunScript(object @this, string script)
        {
            throw new NotImplementedException();
        }

        public void RunScript(object @this, string script, IJsModule module)
        {
            throw new NotImplementedException();
        }

        public Action<IJsExecutionContext> OnExecutionContextDisposing { get; set; }
    }
}