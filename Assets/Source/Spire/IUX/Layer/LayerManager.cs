using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Manages layers.
    /// </summary>
    public class LayerManager : MonoBehaviour
    {
        /// <summary>
        /// List of created layers.
        /// </summary>
        private readonly List<Layer> _layers = new List<Layer>();

        /// <summary>
        /// Top-most layer which is set to Modal.
        /// </summary>
        public Layer ModalLayer { get; private set; }

        /// <summary>
        /// Creates a new layer, 
        /// </summary>
        /// <returns></returns>
        public Layer Request(ILayerable owner)
        {
            var newLayer = new Layer(owner);

            _layers.Add(newLayer);

            Update();

            return newLayer;
        }

        /// <summary>
        /// Release an existing layer.
        /// </summary>
        /// <param name="layer">The Layer to remove.</param>
        public void Release(Layer layer)
        {
            _layers.Remove(layer);

            Update();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public void Update()
        {
            ModalLayer = null;

            for (var i = _layers.Count - 1; i >= 0; --i)
            {
                var layer = _layers[i];
                if (layer.Owner != null
                    && layer.Owner.IsVisible
                    && layer.Owner.LayerMode == LayerMode.Modal)
                {
                    ModalLayer = layer;
                    return;
                }
            }
        }
    }
}