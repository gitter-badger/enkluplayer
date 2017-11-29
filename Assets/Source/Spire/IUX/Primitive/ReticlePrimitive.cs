using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic building block for reticle.
    /// </summary>
    public class ReticlePrimitive
    {
        /// <summary>
        /// Parent.
        /// </summary>
        private Widget _parent;

        /// <summary>
        /// Renderer.
        /// </summary>
        public readonly ReticleRenderer Renderer;

        /// <summary>
        /// Parent widget.
        /// </summary>
        /// <summary>
        /// Gets/sets text primitive parent.
        /// </summary>
        public Widget Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                Renderer.transform.parent = null;
                _parent = value;

                if (null != _parent)
                {
                    Renderer.transform.SetParent(
                        _parent.GameObject.transform,
                        false);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">Configuration.</param>
        public ReticlePrimitive(WidgetConfig config)
        {
            Renderer = Object.Instantiate(config.Reticle);
        }

        /// <summary>
        /// Updates the reticle.
        /// </summary>
        /// <param name="rotation">Rotation of the reticle.</param>
        /// <param name="spread">Spread of the reticle.</param>
        /// <param name="scale">Reticle scale.</param>
        /// <param name="centerAlpha">Alpha of center point.</param>
        public void Update(
            float rotation,
            float spread,
            float scale,
            float centerAlpha)
        {
            Renderer.Refresh(rotation, spread, scale, centerAlpha);
        }
    }
}