using System.Collections.Generic;
using System.Linq;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Creates Test Vine/Behavior components.
    /// </summary>
    public class TestScriptFactory : IScriptFactory
    {
        private struct Record
        {
            public Widget Widget;
            public EnkluScript EnkluScript;
        }
        
        private readonly Dictionary<Record, TestVineScript> _vineCache = new Dictionary<Record, TestVineScript>();
        private readonly Dictionary<Record, TestBehaviorScript> _behaviorCache = new Dictionary<Record, TestBehaviorScript>();
        
        /// <inheritdoc />
        public VineScript Vine(Widget widget, EnkluScript script)
        {
            Record record = new Record
            {
                Widget = widget,
                EnkluScript = script
            };
            
            var component = new TestVineScript(script);

            _vineCache[record] = component;

            return component;
        }

        /// <inheritdoc />
        public BehaviorScript Behavior(
            Widget widget,
            IElementJsCache jsCache,  
            UnityScriptingHost host,
            EnkluScript script)
        {
            Record record = new Record
            {
                Widget = widget,
                EnkluScript = script
            };
            
            var component = new TestBehaviorScript(script);

            _behaviorCache[record] = component;
            
            return component;
        }

        /// <summary>
        /// Gets a Vine component that was given out.
        /// </summary>
        public TestVineScript GetVine(Widget widget, EnkluScript script)
        {
            var entry =  _vineCache.First(kvp => kvp.Key.Widget == widget && kvp.Key.EnkluScript == script);
            return entry.Value;
        }

        /// <summary>
        /// Gets a Behavior component that was given out.
        /// </summary>
        public TestBehaviorScript GetBehavior(Widget widget, EnkluScript script)
        {
            var entry =  _behaviorCache.First(kvp => kvp.Key.Widget == widget && kvp.Key.EnkluScript == script);
            return entry.Value;
        }
    }
}