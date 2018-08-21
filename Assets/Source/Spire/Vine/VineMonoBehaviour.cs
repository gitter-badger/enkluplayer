using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public class VineMonoBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Parsed script.
        /// </summary>
        private ElementDescription _description;

        /// <summary>
        /// The element created.
        /// </summary>
        private Element _element;

        /// <summary>
        /// True iff Enter() has been called but not Exit().
        /// </summary>
        private bool _isEntered;

        /// <summary>
        /// Imports from vine.
        /// </summary>
        [Inject]
        public VineImporter Importer { get; set; }

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; set; }

        /// <summary>
        /// The parent of the script.
        /// </summary>
        public Element Parent { get; private set; }

        /// <summary>
        /// SpireScript.
        /// </summary>
        public SpireScript Script { get; private set; }
        
        /// <summary>
        /// Initializes script.
        /// </summary>
        public bool Initialize(Element parent, SpireScript script)
        {
            Parent = parent;
            Script = script;
            Script.OnReady.OnSuccess(_ =>
            {
                DestroyElements();
                Import(script);

                if (_isEntered)
                {
                    CreateElements();
                }
            });

            return Import(script);
        }

        /// <summary>
        /// Runs script.
        /// </summary>
        public bool Enter()
        {
            _isEntered = true;

            return CreateElements();
        }
        
        /// <summary>
        /// Destroys component and created elements.
        /// </summary>
        public void Exit()
        {
            _isEntered = false;

            DestroyElements();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDestroy()
        {
            Exit();
        }

        /// <summary>
        /// Imports script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        private bool Import(SpireScript script)
        {
            Log.Info(this, "Importing script.");

            try
            {
                _description = Importer.Parse(Script.Source);
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
        /// Creates elements.
        /// </summary>
        private bool CreateElements()
        {
            if (null == _description)
            {
                return false;
            }

            _element = Elements.Element(_description);

            Parent.AddChild(_element);

            return true;
        }

        /// <summary>
        /// Cleans up elements.
        /// </summary>
        private void DestroyElements()
        {
            if (null != _element)
            {
                _element.Destroy();
                _element = null;
            }
        }
    }
}