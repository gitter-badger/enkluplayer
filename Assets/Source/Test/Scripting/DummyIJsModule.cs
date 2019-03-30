using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class DummyIJsModule : IJsModule
    {
        public T GetExportedValue<T>(string name)
        {
            throw new System.NotImplementedException();
        }

        public string ModuleId { get; private set; }
    }
}