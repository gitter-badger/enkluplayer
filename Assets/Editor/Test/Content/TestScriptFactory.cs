using System.Collections.Generic;
using System.Linq;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Creates Test Vine/Behavior components.
    /// </summary>
    public class TestScriptFactory : IScriptFactory
    {
        private struct Record
        {
            public Element Element;
            public EnkluScript EnkluScript;
        }
        
        private readonly Dictionary<Record, TestVineScript> _vineCache = new Dictionary<Record, TestVineScript>();
        private readonly Dictionary<Record, TestBehaviorScript> _behaviorCache = new Dictionary<Record, TestBehaviorScript>();
        
        /// <inheritdoc />
        public VineScript Vine(Element element, EnkluScript script)
        {
            Record record = new Record
            {
                Element = element,
                EnkluScript = script
            };
            
            var component = new TestVineScript(script);

            _vineCache[record] = component;

            return component;
        }

        /// <inheritdoc />
        public BehaviorScript Behavior(
            IJsExecutionContext jsContext,
            Element element,
            EnkluScript script)
        {
            Record record = new Record
            {
                Element = element,
                EnkluScript = script
            };
            
            var component = new TestBehaviorScript(script);

            _behaviorCache[record] = component;
            
            return component;
        }

        /// <summary>
        /// Gets a Vine component that was given out.
        /// </summary>
        public TestVineScript GetVine(Element element, EnkluScript script)
        {
            var entry =  _vineCache.First(kvp => kvp.Key.Element == element && kvp.Key.EnkluScript == script);
            return entry.Value;
        }

        /// <summary>
        /// Gets a Behavior component that was given out.
        /// </summary>
        public TestBehaviorScript GetBehavior(Element element, EnkluScript script)
        {
            var entry =  _behaviorCache.First(kvp => kvp.Key.Element == element && kvp.Key.EnkluScript == script);
            return entry.Value;
        }
    }
}