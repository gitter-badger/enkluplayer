using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates elements from a vine typed in the inspector..
    /// </summary>
    public class VineRawMonoBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Timeout since last update to try to create elements.
        /// </summary>
        private const float TIMEOUT_SEC = 0.1f;

        /// <summary>
        /// Last vine value.
        /// </summary>
        private string _lastVine;

        /// <summary>
        /// Vine we ended up parsing and baking out.
        /// </summary>
        private string _bakedVine;

        /// <summary>
        /// Time at which the vine was last updated.
        /// </summary>
        private DateTime _lastChange = DateTime.MinValue;

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; private set; }

        /// <summary>
        /// The created element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// The vine.
        /// </summary>
        [TextArea(10, 100)]
        public string Vine;

        /// <summary>
        /// Imports from vine.
        /// </summary>
        [Inject]
        public VineImporter Importer { get; set; }

        /// <summary>
        /// Called when element has been created.
        /// </summary>
        public Action<Element> OnElementCreated;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            Update();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            if (null != Element)
            {
                Element.Destroy();
                Element = null;
            }

            _lastChange = DateTime.MinValue;
            _bakedVine = _lastVine = null;
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            var now = DateTime.Now;
            if (Vine != _lastVine)
            {
                _lastChange = now;
                _lastVine = Vine;
            }

            if (now.Subtract(_lastChange).TotalSeconds > TIMEOUT_SEC
                && _bakedVine != Vine)
            {
                _bakedVine = Vine;

                ElementDescription description;
                try
                {
                    description = Importer.ParseSync(Vine);
                }
                catch (Exception exception)
                {
                    Log.Error(this, exception);
                    return;
                }

                if (null != Element)
                {
                    Element.Destroy();
                    Element = null;
                }
                
                Element = Elements.Element(description);
                
                if (null != OnElementCreated)
                {
                    OnElementCreated(Element);
                }
            }
        }
    }
}