using System.Collections;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <inheritdoc />
    public class ScriptFactory : IScriptFactory
    {
        /// <summary>
        /// Vine importer.
        /// </summary>
        [Inject]
        public VineImporter VineImporter { get; set; }
        
        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory ElementFactory { get; set; }
        
        /// <summary>
        /// Creates ElementJs instances
        /// </summary>
        [Inject]
        public IElementJsFactory ElementJsFactory { get; set; }
        
        /// <inheritdoc />
        public VineScript CreateVineComponent(GameObject root, Element element, EnkluScript script)
        {
            var component = root.AddComponent<VineMonoBehaviour>();
            component.Initialize(element, script, VineImporter, ElementFactory);
            
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
            var component = root.AddComponent<EnkluScriptElementBehavior>();
            component.Initialize(jsCache, ElementJsFactory, host, script, element);

            return component;
        }
    }
}