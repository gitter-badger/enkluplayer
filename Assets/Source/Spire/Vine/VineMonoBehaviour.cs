using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public class VineMonoBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Imports from vine.
        /// </summary>
        private readonly VineImporter _importer = new VineImporter(new JsVinePreProcessor());

        /// <summary>
        /// Parsed script.
        /// </summary>
        private ElementDescription _description;

        /// <summary>
        /// The element created.
        /// </summary>
        private Element _element;
        
        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; private set; }
        
        /// <summary>
        /// SpireScript.
        /// </summary>
        public SpireScript Script { get; private set; }
        
        /// <summary>
        /// Initializes script.
        /// </summary>
        public bool Initialize(SpireScript script)
        {
            Script = script;

            try
            {
                _description = _importer.Parse(Script.Source);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not parse {0} : {1}.",
                    script,
                    exception);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Runs script.
        /// </summary>
        public bool Enter()
        {
            if (null == _description)
            {
                return false;
            }
            
            _element = Elements.Element(_description);

            return true;
        }

        /// <summary>
        /// Destroys component and created elements.
        /// </summary>
        public void Exit()
        {
            if (null != _element)
            {
                _element.Destroy();
                _element = null;
            }
        }
    
        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDestroy()
        {
            Exit();
        }
    }
}