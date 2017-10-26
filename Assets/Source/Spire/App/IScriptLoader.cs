using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    public interface IScriptLoader
    {
        IAsyncToken<string> Load(ScriptData script);
    }
}