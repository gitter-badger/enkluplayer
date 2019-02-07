using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    public interface IScriptAssembly
    {
        
    }
    
    public interface IScriptAssembler
    {
        event Action<Script[], Script[]> OnScriptsUpdated;
        
        void Setup(Widget widget);
        
        void Teardown();
    }
}