using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class AutoController : InjectableMonoBehaviour
    {
        /// <summary>
        /// Last vine value.
        /// </summary>
        private string _lastVine;

        // show
        //  -> if create
        //      -> initialize
        // hide
        //  -> uninitialize

        // update
        //  -> if create
        //      -> uninitialize
        //      -> initialize
        
        private object _context;
        private bool _isShowing;

        [TextArea(10, 100)]
        public string Vine;

        [Inject]
        public VineImporter Importer { get; set; }

        [Inject]
        public IElementFactory Elements { get; set; }

        public Element Element { get; private set; }

        public void Show(object context)
        {
            var element = Create();
            if (null != element)
            {
                _context = context;
                _isShowing = true;

                Initialize(element, _context);
            }
            else
            {
                throw new Exception("Could not create element.");
            }
        }

        public void Hide()
        {
            Uninitialize();

            _isShowing = false;
        }

        private Element Create()
        {
            try
            {
                return Elements.Element(Importer.Parse(Vine));
            }
            catch (Exception exception)
            {
                Log.Error(this, exception);
                return null;
            }
        }

        protected virtual void Initialize(Element element, object context)
        {
            Element = element;
            
            InjectElementsAttribute.InjectElements(this, Element);
        }

        protected virtual void Uninitialize()
        {
            if (null != Element)
            {
                Element.Destroy();
            }
        }

        private void Update()
        {
            if (_isShowing && Vine != _lastVine)
            {
                _lastVine = Vine;

                var element = Create();
                if (null != element)
                {
                    Uninitialize();
                    Initialize(element, _context);
                }
            }
        }
    }
}