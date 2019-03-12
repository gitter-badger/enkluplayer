using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Responsible for building Script instances for a given Widget. 
    /// </summary>
    public interface IScriptAssembler
    {
        /// <summary>
        /// Invoked whenever scripts change with the old & new scripts sent as payloads.
        /// </summary>
        event Action<Script[], Script[]> OnScriptsUpdated;
        
        /// <summary>
        /// Setups up Scripts for the given Element.
        /// </summary>
        /// <param name="widget">The Widget.</param>
        void Setup(Element element);
        
        /// <summary>
        /// Tears down the Assembler.
        /// </summary>
        void Teardown();
    }
}