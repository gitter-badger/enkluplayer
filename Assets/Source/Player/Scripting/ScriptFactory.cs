using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using Enklu.Orchid;
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
        /// Constructor.
        /// </summary>
        public ScriptFactory(
            IElementFactory elementFactory,
            VineImporter vineImporter)
        {
            _elementFactory = elementFactory;
            _vineImporter = vineImporter;
        }
        
        /// <inheritdoc />
        public VineScript Vine(Element element, EnkluScript script)
        {
            if (element == null)
            {
                throw new Exception("Vine attached to non-widget?!");
            }
            
            return new VineScript(element, script, _vineImporter, _elementFactory);
        }

        /// <inheritdoc />
        public BehaviorScript Behavior(
            IElementJsCache jsCache,
            IJsExecutionContext jsContext,
            Element element,
            EnkluScript script)
        {
            if (element == null)
            {
                throw new Exception("Vine attached to non-widget?!");
            }

            return new BehaviorScript(jsCache, jsContext, script, element);
        }
    }
}