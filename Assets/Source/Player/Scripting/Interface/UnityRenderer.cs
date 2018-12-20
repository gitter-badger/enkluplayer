using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// A Unity implementation of IRenderer.
    /// </summary>
    public class UnityRenderer : IRenderer
    {
        /// <summary>
        /// Backing Unity Renderer.
        /// </summary>
        private Renderer _renderer;

        /// <summary>
        /// Backing Material.
        /// </summary>
        private IMaterial _material;

        /// <inheritdoc />
        public IMaterial SharedMaterial
        {
            get { return _material; }
            set
            {
                var unityMat = value as UnityMaterial;
                if (unityMat == null)
                {
                    throw new Exception("Trying to use non UnityMaterial with UnityRenderer");
                }

                _material = value;
                _renderer.sharedMaterial = unityMat.Material;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer"></param>
        public UnityRenderer(Renderer renderer)
        {
            _renderer = renderer;
            _material = new UnityMaterial(renderer.sharedMaterial);
        }
    }
}