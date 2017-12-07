using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages layers.
    /// </summary>
    public class LayerManager : MonoBehaviour, ILayerManager
    {
        /// <summary>
        /// List of created layers.
        /// </summary>
        private readonly List<Layer> _layers = new List<Layer>();

        /// <inheritdoc cref="ILayerManager"/>
        public Layer ModalLayer { get; private set; }

        /// <inheritdoc cref="ILayerManager"/>
        public Layer Request(ILayerable owner)
        {
            var newLayer = new Layer(owner);

            _layers.Add(newLayer);

            Update();

            return newLayer;
        }

        /// <inheritdoc cref="ILayerManager"/>
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
                    && layer.Owner.Visible
                    && layer.Owner.LayerMode == LayerMode.Modal)
                {
                    ModalLayer = layer;
                    return;
                }
            }
        }
    }
}