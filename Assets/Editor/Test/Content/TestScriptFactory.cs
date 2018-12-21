using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Creates Test Vine/Behavior components.
    /// </summary>
    public class TestScriptFactory : IScriptFactory
    {
        private readonly Dictionary<EnkluScript, TestVineMonoBehaviour> _vineCache = new Dictionary<EnkluScript, TestVineMonoBehaviour>();
        private readonly Dictionary<EnkluScript, TestBehaviorMonoBehaviour> _behaviorCache = new Dictionary<EnkluScript, TestBehaviorMonoBehaviour>();
        
        /// <inheritdoc />
        public VineScript Vine(Widget widget, EnkluScript script)
        {
            var existing = GetVine(script);

            if (existing != null)
            {
                return existing;
            }
            
            var component = widget.GameObject.AddComponent<TestVineMonoBehaviour>();
            _vineCache.Add(script, component);

            return component;
        }

        /// <inheritdoc />
        public BehaviorScript Behavior(
            Widget widget,
            IElementJsCache jsCache,  
            UnityScriptingHost host,
            EnkluScript script)
        {
            var existing = GetBehavior(script);

            if (existing != null)
            {
                return existing;
            }
            
            var component = widget.GameObject.AddComponent<TestBehaviorMonoBehaviour>();
            _behaviorCache.Add(script, component);

            return component;
        }

        /// <summary>
        /// Gets a Vine component that was given out.
        /// </summary>
        public TestVineMonoBehaviour GetVine(EnkluScript script)
        {
            TestVineMonoBehaviour component;
            _vineCache.TryGetValue(script, out component);
            return component;
        }

        /// <summary>
        /// Gets a Behavior component that was given out.
        /// </summary>
        public TestBehaviorMonoBehaviour GetBehavior(EnkluScript script)
        {
            TestBehaviorMonoBehaviour component;
            _behaviorCache.TryGetValue(script, out component);
            return component;
        }
    }
}