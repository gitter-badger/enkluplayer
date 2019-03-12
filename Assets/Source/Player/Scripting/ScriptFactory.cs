using System;
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
        private readonly VineImporter _vineImporter;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elementFactory;

        /// <summary>
        /// Creates ElementJs instances
        /// </summary>
        private readonly IElementJsFactory _elementJsFactory;

        public ScriptFactory(
            IElementFactory elementFactory, 
            IElementJsFactory elementJsFactory, 
            VineImporter vineImporter)
        {
            _elementFactory = elementFactory;
            _elementJsFactory = elementJsFactory;
            _vineImporter = vineImporter;
        }
        
        /// <inheritdoc />
        public VineScript Vine(Widget widget, EnkluScript script)
        {
            if (widget == null)
            {
                throw new Exception("Vine attached to non-widget?!");
            }
            
            return new VineScript(widget, script, _vineImporter, _elementFactory);
        }

        /// <inheritdoc />
        public BehaviorScript Behavior(
            Widget widget,
            IElementJsCache jsCache,  
            UnityScriptingHost host, 
            EnkluScript script)
        {
            if (widget == null)
            {
                throw new Exception("Vine attached to non-widget?!");
            }

            return new BehaviorScript(jsCache, _elementJsFactory, host, script, widget);
        }
    }
}