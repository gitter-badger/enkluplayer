using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
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
        /// Imports from vine.
        /// </summary>
        private readonly VineImporter _importer = new VineImporter(new JsVinePreProcessor());

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
        /// The element created.
        /// </summary>
        private Element _element;

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; private set; }

        /// <summary>
        /// The vine.
        /// </summary>
        [TextArea(10, 100)]
        public string Vine;
        
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
                    description = _importer.Parse(Vine);
                }
                catch (Exception exception)
                {
                    Log.Error(this, exception);
                    return;
                }

                if (null != _element)
                {
                    _element.Destroy();
                    _element = null;
                }
                
                _element = Elements.Element(description);

                var widget = _element as Widget;
                if (null != widget)
                {
                    widget.GameObject.transform.parent = transform;
                }
            }
        }
    }
}