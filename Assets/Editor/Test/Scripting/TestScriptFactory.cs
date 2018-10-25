using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Test.Vine;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// Creates Test Vine/Behavior components.
    /// </summary>
    public class TestScriptFactory : IScriptFactory
    {
        private Dictionary<EnkluScript, TestVineMonoBehaviour> _vineCache = new Dictionary<EnkluScript, TestVineMonoBehaviour>();
        private Dictionary<EnkluScript, TestBehaviorMonoBehaviour> _behaviorCache = new Dictionary<EnkluScript, TestBehaviorMonoBehaviour>();
        
        /// <inheritdoc />
        public VineScript CreateVineComponent(GameObject root, Element element, EnkluScript script)
        {
            var component = root.AddComponent<TestVineMonoBehaviour>();
            _vineCache.Add(script, component);

            return component;
        }

        /// <inheritdoc />
        public BehaviorScript CreateBehaviorComponent(
            GameObject root, 
            IElementJsCache jsCache,  
            UnityScriptingHost host,
            EnkluScript script, 
            Element element)
        {
            var component = root.AddComponent<TestBehaviorMonoBehaviour>();
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