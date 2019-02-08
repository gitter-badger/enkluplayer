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
        
        private readonly Dictionary<Record, TestVineMonoBehaviour> _vineCache = new Dictionary<Record, TestVineMonoBehaviour>();
        private readonly Dictionary<Record, TestBehaviorMonoBehaviour> _behaviorCache = new Dictionary<Record, TestBehaviorMonoBehaviour>();
        
        /// <inheritdoc />
        public VineScript Vine(Widget widget, EnkluScript script)
        {
            Record record = new Record
            {
                Widget = widget,
                EnkluScript = script
            };
            
            var component = widget.GameObject.AddComponent<TestVineMonoBehaviour>();
            component.Initialize(widget.Parent, script, null, null);

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
            
            var component = widget.GameObject.AddComponent<TestBehaviorMonoBehaviour>();
            component.Initialize(jsCache, null, host, script, widget);

            _behaviorCache[record] = component;
            
            return component;
        }

        /// <summary>
        /// Gets a Vine component that was given out.
        /// </summary>
        public TestVineMonoBehaviour GetVine(Widget widget, EnkluScript script)
        {
            TestVineMonoBehaviour component;
            var entry =  _vineCache.First(kvp => kvp.Key.Widget == widget && kvp.Key.EnkluScript == script);
            return entry.Value;
        }

        /// <summary>
        /// Gets a Behavior component that was given out.
        /// </summary>
        public TestBehaviorMonoBehaviour GetBehavior(Widget widget, EnkluScript script)
        {
            TestBehaviorMonoBehaviour component;
            var entry =  _behaviorCache.First(kvp => kvp.Key.Widget == widget && kvp.Key.EnkluScript == script);
            return entry.Value;
        }
    }
}