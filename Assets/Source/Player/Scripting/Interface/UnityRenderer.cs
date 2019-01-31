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

        /// <summary>
        /// Whether the cached material is shared.
        /// </summary>
        private bool _shared;

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
        
        /// <inheritdoc />
        public IMaterial Material
        {
            get 
            {
                if (_shared)
                {
                    _shared = false;
                    _material = new UnityMaterial(_renderer.material, _shared);
                }

                return _material;
            }
            set
            {
                var unityMat = value as UnityMaterial;
                if (unityMat == null)
                {
                    throw new Exception("Trying to use non UnityMaterial with UnityRenderer");
                }

                _material = value;
                _renderer.material = unityMat.Material;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer"></param>
        public UnityRenderer(Renderer renderer)
        {
            _renderer = renderer;
            _shared = true;
            _material = new UnityMaterial(renderer.sharedMaterial, _shared);
        }

        /// <summary>
        /// Creates a UnityMaterial around a Material instance.
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private UnityMaterial CreateMaterial(Material material)
        {
            return new UnityMaterial(_renderer.material, _shared);
        }
    }
}