using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// An interface to Unity's Material system.
    /// Properties of an Element's shared material can be modified.
    /// </summary>
    public class MaterialJsApi
    {
        /// <summary>
        /// Underlying Unity Renderer to modify.
        /// </summary>
        private readonly Renderer _renderer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer"></param>
        public MaterialJsApi(Renderer renderer)
        {
            _renderer = renderer;
        }

        /// <summary>
        /// Sets a float material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setFloat(string param, float value)
        {
            _renderer.sharedMaterial.SetFloat(param, value);
        }

        /// <summary>
        /// Sets an integer material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setInt(string param, int value)
        {
            _renderer.sharedMaterial.SetInt(param, value);
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(string param, Vec3 value)
        {
            _renderer.sharedMaterial.SetVector(param, value.ToVector());
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3 & alpha.
        /// TODO: Add in a Vec4 and use that instead.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <param name="alpha"></param>
        public void setVector(string param, Vec3 value, float alpha)
        {
            _renderer.sharedMaterial.SetVector(
                param, 
                new Vector4(value.x, value.y, value.z, alpha));
        }

        /// <summary>
        /// Sets a named shader pass enabled/disabled.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="enabled"></param>
        public void setShaderPass(string pass, bool enabled)
        {
            _renderer.sharedMaterial.SetShaderPassEnabled(pass, enabled);
        }
    }
}