using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Unity implementation of IMaterial.
    /// </summary>
    public class UnityMaterial : IMaterial
    {
        /// <summary>
        /// Whether the backing material is shared or not.
        /// </summary>
        private bool _shared;
        
        /// <summary>
        /// Backing material.
        /// </summary>
        public Material Material { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="shared"></param>
        public UnityMaterial(Material material, bool shared)
        {
            Material = material;
            _shared = shared;
        }
        
        /// <inheritdoc />
        public float GetFloat(string param)
        {
            return Material.GetFloat(param);
        }

        /// <inheritdoc />
        public void SetFloat(string param, float value)
        {
            Material.SetFloat(param, value);
        }

        /// <inheritdoc />
        public int GetInt(string param)
        {
            return Material.GetInt(param);
        }

        /// <inheritdoc />
        public void SetInt(string param, int value)
        {
            Material.SetInt(param, value);
        }

        /// <inheritdoc />
        public Vec3 GetVec3(string param)
        {
            var v4 = Material.GetVector(param);
            return new Vec3(v4.x, v4.y, v4.z);
        }

        /// <inheritdoc />
        public void SetVec3(string param, Vec3 value)
        {
            Material.SetVector(param, value.ToVector());
        }

        /// <inheritdoc />
        public Col4 GetCol4(string param)
        {
            var v4 = Material.GetVector(param);
            return new Col4(v4.x, v4.y, v4.z, v4.w);
        }

        /// <inheritdoc />
        public void SetCol4(string param, Col4 value)
        {
            Material.SetVector(param, value.ToColor());
        }

        public void Teardown()
        {
            if (!_shared)
            {
                Object.Destroy(Material);
            }
        }
    }
}