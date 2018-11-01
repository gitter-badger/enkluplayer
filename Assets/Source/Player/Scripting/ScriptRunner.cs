using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptRunner
    {
        public enum SetupState
        {
            Error = -1,
            None,
            Parsing,
            Done
        }
        
        private IScriptManager _scriptManager; 

        public ScriptRunner(IScriptManager scriptManager)
        {
            _scriptManager = scriptManager;
        }
        
        public void AddWidget(Widget widget)
        {
            
        }

        public SetupState GetSetupState(Widget widget)
        {
            
        }

        public void ParseSync()
        {
            
        }

        public IAsyncToken<Void> Parse()
        {
            throw new NotImplementedException();
        }
    }
}